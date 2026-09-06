using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 幸运房东 — 工资系统初始遗物
	/// 战斗胜利 +100 工资；每 4 场战斗房东收租（房租固定 350，不再成长）。
	/// 付得起：扣房租并随机奖励；付不起：卡组加入一张【债务】，且该次应缴房租累积为欠款，
	/// 下次收租需一并补缴，否则继续累积。
	/// 遗物左下角显示收租倒计时 0,1,2,3；工资数值显示在能量槽左侧的工资槽。
	/// </summary>
	public sealed class LuckyLandlordRelic : RelicModel
	{
		private const int _salaryGain = 100;
		private const int _collectInterval = 4;

		private int _salary;
		private int _rent = 350;      // 房租固定 350，不再成长
		private int _arrears;         // 累计拖欠的房租（下次收租需一并补缴，付不起继续累积）
		private int _combatCount;
		private string _lastReward = "无";
		private bool _pendingSpin;        // 本次收租已够付，待玩家在战利品页面点击“旋转一次”抽奖
		private bool _liveStreamGiven;   // 直播之路是否已给（SL 持久化，防重复）
		private bool _bwNightGiven;      // BW之夜是否已给（SL 持久化，防重复）
		private bool _lianJiGiven;       // 联机是否已给（SL 持久化，防重复）

		public override RelicRarity Rarity => RelicRarity.Starter;

		public override bool ShowCounter => IsMutable;
		// 遗物左下角显示收租倒计时 0,1,2,3（每 4 场胜利收租一次，CombatCount 满 4 归零）
		public override int DisplayAmount => _combatCount;

		// 待玩家点击的“旋转一次”抽奖（SL 后保留，防止收租后 SL 丢失抽奖机会）
		[SavedProperty]
		public bool PendingSpin
		{
			get => _pendingSpin;
			set { AssertMutable(); _pendingSpin = value; }
		}

		// 直播之路是否已给过（SL 后保留，防止 SL 后第一个事件被再次替换成直播之路）
		[SavedProperty]
		public bool LiveStreamGiven
		{
			get => _liveStreamGiven;
			set { AssertMutable(); _liveStreamGiven = value; }
		}

		// BW之夜是否已给过（第三层第一个事件，SL 后保留防重复）
		[SavedProperty]
		public bool BwNightGiven
		{
			get => _bwNightGiven;
			set { AssertMutable(); _bwNightGiven = value; }
		}

		// 联机是否已给过（第二层第一个事件，SL 后保留防重复）
		[SavedProperty]
		public bool LianJiGiven
		{
			get => _lianJiGiven;
			set { AssertMutable(); _lianJiGiven = value; }
		}

		[SavedProperty]
		public int Salary
		{
			get => _salary;
			set
			{
				AssertMutable();
				_salary = value;
				InvokeDisplayAmountChanged();
				// 同步显示变量（SL 恢复时反射设置字段，DynamicVars 不会自动同步）
				if (DynamicVars?["Salary"] is IntVar s) s.BaseValue = value;
			}
		}

		[SavedProperty]
		public int Rent
		{
			get => _rent;
			set
			{
				AssertMutable();
				_rent = value;
				if (DynamicVars?["Rent"] is IntVar r) r.BaseValue = value;
			}
		}

		// 累计拖欠的房租（SL 持久化：下次收租需一并补缴，付不起继续累积）
		[SavedProperty]
		public int Arrears
		{
			get => _arrears;
			set
			{
				AssertMutable();
				_arrears = value;
				if (DynamicVars?["Arrears"] is IntVar a) a.BaseValue = value;
			}
		}

		[SavedProperty]
		public int CombatCount
		{
			get => _combatCount;
			set
			{
				AssertMutable();
				_combatCount = value;
				InvokeDisplayAmountChanged(); // 刷新遗物左下角 0-3 倒计时（与 Salary setter 同机制）
				if (DynamicVars?["Countdown"] is IntVar cd) cd.BaseValue = _collectInterval - value;
			}
		}

		[SavedProperty]
		public string LastRewardText
		{
			get => _lastReward;
			set
			{
				AssertMutable();
				_lastReward = value;
				if (DynamicVars?["LastReward"] is StringVar l) l.StringValue = value;
			}
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("Salary", 0m),
			new IntVar("Rent", 350m),
			new IntVar("Arrears", 0m),
			new IntVar("Countdown", 4m),
			new StringVar("LastReward", "无")
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				if (!IsMutable) yield break; // 图鉴（模板）不显示状态悬浮窗
				var desc = new LocString("relics", "LUCKY_LANDLORD_RELIC.progressDesc");
				DynamicVars.AddTo(desc); // 注入 Salary/Rent/Arrears/Countdown/LastReward
				yield return new HoverTip(desc); // 单参构造 → Title=null，无标题行

				// 词条链接：工资机制说明（与卡牌一致）
				var salaryTitle = new LocString("cards", "SALARY_MECHANICS_TITLE");
				var salaryDesc = new LocString("cards", "SALARY_MECHANICS_DESC");
				yield return new HoverTip(salaryTitle, salaryDesc);
			}
		}

		// 战斗胜利奖励生成时：把待定的“旋转一次”奖励加入战利品
		public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			if (player != Owner) return false;
			bool added = false;
			if (PendingSpin)
			{
				PendingSpin = false;
				rewards.Add(new SpinReward(player, this));
				added = true;
			}
			return added;
		}

		// 战斗胜利：+100 工资，每 4 场胜利房东收租
		public override async Task AfterCombatVictory(CombatRoom room)
		{
			if (room.Encounter.RoomType == RoomType.Event) return;
			if (!LocalContext.IsMe(Owner)) return;
			if (Owner.Creature == null || Owner.Creature.IsDead) return;

			Salary += _salaryGain;
			CombatCount++;

			if (CombatCount >= _collectInterval)
			{
				CombatCount = 0;
				await CollectRent();
			}

			UpdateDynamicVars();
		}

		private async Task CollectRent()
		{
			var player = Owner;
			if (player == null) return;

			Flash();

			// 应缴 = 固定房租 + 累计欠款（拖欠过则需一并补缴）
			int totalDue = Rent + Arrears;
			bool hadArrears = Arrears > 0;

			if (Salary >= totalDue)
			{
				// 付得起（含补缴欠款）：扣全部应缴，欠款清零；不在收租时自动结算，
				// 改为战利品页面添加“旋转一次”，由玩家点击抽奖
				Salary -= totalDue;
				Arrears = 0;
				PendingSpin = true;
				if (hadArrears)
				{
					LastRewardText = "房东已收租（含补缴欠款）！可在战利品中旋转一次";
					ShowBubble("房东收下房租并补缴欠款！战利品里旋转一次抽奖！");
				}
				else
				{
					LastRewardText = "房东已收租！可在战利品中旋转一次";
					ShowBubble("房东收下房租！战利品里旋转一次抽奖！");
				}
			}
			else
			{
				// 恶魔契约：付不起房租时取消收租（只生效一次），本次不扣款、不记欠款
				if (EMoQiYueRelic.TryConsumeSkip(player))
				{
					LastRewardText = "恶魔契约：本次收租已取消";
					ShowBubble("恶魔契约生效！本次收租被取消！");
				}
				else
				{
					// 付不起：本次应缴全部累积为欠款（下次需补缴，否则继续累积），卡组加入一张【债务】
					Arrears += totalDue;
					LastRewardText = $"无力支付房租，获得【债务】（累计欠款 {Arrears}）";
					// 必须用 CreateCard 创建带 owner 的卡（ModelDb.Card 的 canonical/ToMutable 无 owner，CardPileCmd.Add 会抛异常）
					var debt = Owner.RunState.CreateCard<Debt>(Owner);
					await CardPileCmd.Add(debt, PileType.Deck);
					ShowBubble("付不起房租！一张【债务】被塞进了你的卡组……（欠款将在下次收租时补缴）");
				}
			}

			UpdateDynamicVars();
		}

		// 由 SpinReward（战利品页面“旋转一次”）点击时调用
		internal async Task GrantReward(Player player)
		{
			// 使用玩家奖励专用随机流，与游戏奖励系统一致
			var rng = player.PlayerRng.Rewards;
			int roll = rng.NextInt(1, 1001); // 1-1000

			if (roll <= 184) // 18.4% 随机遗物（仅限本角色遗物池；空奖16%取消后均分）
			{
				var relics = player.Character.RelicPool.AllRelics
					.Where(r => r.Rarity != RelicRarity.Starter && r.Rarity != RelicRarity.Event)
					.ToList();
				var relic = rng.NextItem(relics);
				if (relic != null)
				{
					await RelicCmd.Obtain(relic.ToMutable(), player);
					LastRewardText = "随机遗物";
					ShowBubble("房东很满意！获得一个随机遗物！");
				}
				else
				{
					LastRewardText = "无";
				}
			}
			else if (roll <= 398) // 21.4% 100金币
			{
				await PlayerCmd.GainGold(100m, player);
				LastRewardText = "100金币";
				ShowBubble("房东很满意！获得100金币！");
			}
			else if (roll <= 582) // 18.4% 7最大生命（GainMaxHp 会自动回复等量生命）
			{
				if (Owner.Creature != null)
				{
					await CreatureCmd.GainMaxHp(Owner.Creature, 7m);
					LastRewardText = "7点最大生命";
					ShowBubble("房东很满意！获得7点最大生命！");
				}
				else
				{
					LastRewardText = "无";
				}
			}
			else if (roll <= 766) // 18.4% 卡牌奖励：立即弹出卡牌选择界面（战利品已生成，不能挂 Pending）
			{
				LastRewardText = "卡牌奖励";
				ShowBubble("房东很满意！额外送你一次卡牌选择！");
				await RewardsCmd.OfferCustom(player, new List<Reward>
				{
					new CardReward(CardCreationOptions.ForRoom(player, RoomType.Monster), 3, player)
				});
			}
			else if (roll <= 950) // 18.4% 随机药水：立即弹出药水选择界面
			{
				LastRewardText = "随机药水";
				ShowBubble("房东很满意！额外送你一瓶随机药水！");
				await RewardsCmd.OfferCustom(player, new List<Reward>
				{
					new PotionReward(player)
				});
			}
			else // 5% 999金币（隐藏大奖，从1%提升到5%）
			{
				await PlayerCmd.GainGold(999m, player);
				LastRewardText = "999金币（中大奖！）";
				ShowBubble("房东大喜！赏你999金币！！");
			}
		}

		private void UpdateDynamicVars()
		{
			if (DynamicVars == null) return;
			if (DynamicVars["Salary"] is IntVar s) s.BaseValue = Salary;
			if (DynamicVars["Rent"] is IntVar r) r.BaseValue = Rent;
			if (DynamicVars["Arrears"] is IntVar a) a.BaseValue = Arrears;
			if (DynamicVars["Countdown"] is IntVar cd) cd.BaseValue = _collectInterval - CombatCount;
			if (DynamicVars["LastReward"] is StringVar l) l.StringValue = LastRewardText;
		}

		private void ShowBubble(string text)
		{
			try
			{
				if (Owner?.Creature == null) return;
				var bubble = NSpeechBubbleVfx.Create(text, Owner.Creature, 2.5);
				NCombatRoom.Instance?.CombatVfxContainer?.AddChild(bubble);
			}
			catch (Exception e)
			{
				Log.Error($"[LuckyLandlordRelic] 气泡提示失败: {e.Message}");
			}
		}
	}
}
