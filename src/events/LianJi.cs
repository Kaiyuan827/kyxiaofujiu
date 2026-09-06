using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.events
{
	/// <summary>
	/// 联机 — 第二层第一个事件。
	/// 直播打完一局后，和各大主播联机的选择。
	/// </summary>
	public sealed class LianJi : EventModel
	{
		public override bool IsAllowed(IRunState runState)
		{
			return runState.CurrentActIndex == 1;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			// 农神需工资≥100，辉哥需金币≥100，否则选项锁定
			var hasSalary = HasSalary(100);
			var hasGold = Owner != null && Owner.Gold >= 100;
			return new EventOption[]
			{
				hasSalary
					? new EventOption(this, ActNongshen, "LIAN_JI.pages.INITIAL.options.NONGSHEN")
					: new EventOption(this, null, "LIAN_JI.pages.INITIAL.options.NONGSHEN_LOCKED"),
				new EventOption(this, ActWeishen, "LIAN_JI.pages.INITIAL.options.WEISHEN"),
				hasGold
					? new EventOption(this, ActHuige, "LIAN_JI.pages.INITIAL.options.HUIGE")
					: new EventOption(this, null, "LIAN_JI.pages.INITIAL.options.HUIGE_LOCKED"),
				new EventOption(this, ActBingjie, "LIAN_JI.pages.INITIAL.options.BINGJIE"),
			};
		}

		// 工资是否≥指定值（读幸运房东遗物）
		private bool HasSalary(int amount)
		{
			var landlord = Owner?.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
			return landlord != null && landlord.Salary >= amount;
		}

		// 和农神联机：支付100工资，旋转一次（用现成的 SpinReward 战利品按钮，玩家点击后抽奖）
		private async Task ActNongshen()
		{
			SetEventFinished(L10NLookup("LIAN_JI.pages.NONGSHEN.description"));
			if (Owner == null) return;
			var landlord = Owner.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
			if (landlord != null)
			{
				landlord.Salary = Math.Max(0, landlord.Salary - 100);
				// 旋转一次：战利品界面出现"旋转一次"按钮（SpinReward），玩家点击才执行房东抽奖
				await RewardsCmd.OfferCustom(Owner, new List<Reward> { new SpinReward(Owner, landlord) });
			}
		}

		// 和维神联机：获得8金币和一组稀有卡牌奖励（搜刮战利品方式，玩家点击领取）
		private async Task ActWeishen()
		{
			SetEventFinished(L10NLookup("LIAN_JI.pages.WEISHEN.description"));
			if (Owner == null) return;
			// 与副本 Boss 的稀有卡奖励一致：Source=Encounter + RarityOdds=BossEncounter。
			// 这样 CardReward.IconPath 的第一分支（Encounter + BossEncounter）会命中稀有卡图标
			// reward_icon_rare.png；否则回落到 reward_icon_card.png（普通卡牌图标），图标与奖励不符。
			var options = new CardCreationOptions(
					new CardPoolModel[] { Owner.Character.CardPool },
					CardCreationSource.Encounter,
					CardRarityOddsType.BossEncounter) // BossEncounter = 只出稀有卡
				.WithFlags(CardCreationFlags.NoUpgradeRoll);
			await RewardsCmd.OfferCustom(Owner, new List<Reward>
			{
				new GoldReward(8, Owner),
				new CardReward(options, 3, Owner),
			});
		}

		// 和辉哥联机：失去100金币，获得一个随机战未来遗物（战利品方式点击领取）
		private async Task ActHuige()
		{
			SetEventFinished(L10NLookup("LIAN_JI.pages.HUIGE.description"));
			if (Owner == null) return;
			await PlayerCmd.LoseGold(100m, Owner);

			RelicModel? relic;
			var hasMystic = Owner.Relics.OfType<MysticLighter>().Any();
			var hasChemical = Owner.Relics.OfType<ChemicalX>().Any();
			if (!hasMystic || !hasChemical)
			{
				// 战未来遗物：神秘打火机 / 化学物X，随机给一个还没拥有的
				var candidates = new List<RelicModel>();
				if (!hasMystic) candidates.Add(ModelDb.Relic<MysticLighter>());
				if (!hasChemical) candidates.Add(ModelDb.Relic<ChemicalX>());
				relic = Rng.NextItem(candidates);
			}
			else
			{
				// 两个战未来遗物都有了：随机一个稀有遗物
				var rares = Owner.Character.RelicPool.AllRelics.Where(r => r.Rarity == RelicRarity.Rare).ToList();
				relic = Rng.NextItem(rares);
			}

			if (relic != null)
			{
				await RewardsCmd.OfferCustom(Owner, new List<Reward> { new RelicReward(relic.ToMutable(), Owner) });
			}
		}

		// 和病姐联机：删除两张牌，获得『疑虑』（诅咒直接加入卡组，不走战利品）
		private async Task ActBingjie()
		{
			SetEventFinished(L10NLookup("LIAN_JI.pages.BINGJIE.description"));
			if (Owner == null) return;
			var cards = (await CardSelectCmd.FromDeckForRemoval(
				player: Owner,
				prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2),
				filter: c => true)).ToList();
			await CardPileCmd.RemoveFromDeck(cards);
			await CardPileCmd.AddCurseToDeck<Doubt>(Owner);
		}
	}
}
