using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.monsters;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.commands
{
	public static class ZhengHunCmd
	{
		// 征婚气泡提示（每次征婚随机显示一句）
		private static readonly string[] _marryBubbleTexts =
		{
			"用萎靡给蛇花小姐减去8点力量",
			"征婚，要求：女方尖塔500h+",
			"真不压抑啊，我哪里压抑了",
			"她真好看，想吃她的大份",
		};

		private static void ShowMarryBubble(Player player)
		{
			try
			{
				if (player.Creature == null) return;
				var text = _marryBubbleTexts[GD.RandRange(0, _marryBubbleTexts.Length - 1)];
				var bubble = NSpeechBubbleVfx.Create(text, player.Creature, 2.5);
				NCombatRoom.Instance?.CombatVfxContainer?.AddChild(bubble);
			}
			catch (System.Exception e)
			{
				Log.Error($"[ZhengHunCmd] 气泡提示失败: {e.Message}");
			}
		}

		/// <summary>
		/// 强制刷新蛇花小姐的 Hitbox（悬停判定区域）
		/// </summary>
		private static void RefreshHitbox(Creature creature)
		{
			var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
			if (creatureNode == null) return;

			try
			{
				var hitbox = creatureNode.GetNode<Control>("%Hitbox");
				var bounds = creatureNode.Visuals.Bounds;

				if (hitbox != null && bounds != null)
				{
					// 直接复制 Bounds 的位置和大小到 Hitbox
					hitbox.Size = bounds.Size;
					hitbox.GlobalPosition = bounds.GlobalPosition;

					// ✅ 使用 Control 嵌套枚举的正确路径
					hitbox.MouseFilter = Control.MouseFilterEnum.Stop;
					hitbox.FocusMode = Control.FocusModeEnum.All;

					Log.Info($"=== 蛇花小姐 Hitbox 已更新: Size={hitbox.Size}, Pos={hitbox.GlobalPosition} ===");
				}
			}
			catch (System.Exception ex)
			{
				Log.Error($"=== 刷新 Hitbox 失败: {ex.Message} ===");
			}
		}

		public static async Task Marry(PlayerChoiceContext choiceContext, Player player, int amount)
		{
			if (amount <= 0) return;

			// 检查是否被禁止征婚
			if (player.Creature.HasPower<NoMarryPower>())
			{
				// 对齐“战斗专注/不可抽牌”：被禁止时触发能力图标闪烁提示“禁止征婚”
				player.Creature.GetPower<NoMarryPower>()?.NotifyBlocked();
				return;
			}

			// 检查是否有“压抑形态”能力，征婚效果翻倍
			var yayixingtai = player.Creature.GetPower<YayixingtaiPower>();
			if (yayixingtai != null)
			{
				amount *= yayixingtai.GetMultiplier();
			}

			// 三十岁的男人：征婚时获得 2 倍征婚值的格挡（每层 2 倍，可叠加）
			// 被动能力触发（非卡牌打出来源）→ Unpowered，不吃敏捷/脆弱加成
			var sanshisui = player.Creature.GetPower<SanshisuidenanrenPower>();
			if (sanshisui != null)
			{
				await CreatureCmd.GainBlock(player.Creature, amount * sanshisui.Amount, ValueProp.Unpowered, null);
			}

			var combatState = player.Creature.CombatState;
			if (combatState == null) return;

			// ---- 征婚扣除玩家HP：每征婚1扣2HP，最低1HP ----
			// 不为你而死：征婚不再消耗生命（全额有效征婚）
			bool freeMarry = player.Creature.HasPower<BuweinierersiPower>();

			int playerHp = player.Creature.CurrentHp;
			int hpCost = amount * 2;
			int actualCost = freeMarry ? 0 : System.Math.Min(hpCost, playerHp - 1);
			// 有效征婚次数 = 实际支付HP / 2（向下取整）
			// 免费征婚（不扣血）：不加生命，只加征婚值
			int effectiveAmount = actualCost / 2;
			if (actualCost > 0)
			{
				await CreatureCmd.Damage(choiceContext, player.Creature, actualCost,
					ValueProp.Unblockable | ValueProp.Unpowered, null, null, null);
			}

			// ---- 征婚气泡提示（每次征婚随机显示一句，给玩家即时反馈）----
			ShowMarryBubble(player);

			var existingCreature = combatState.Allies
				.FirstOrDefault(c => c.Monster is SheHuaXiaoJieMonster && c.PetOwner == player && c.IsAlive);

			if (existingCreature != null && existingCreature.Monster is SheHuaXiaoJieMonster existing)
			{
				// 有效征婚：增加HP
				for (int i = 0; i < effectiveAmount; i++)
				{
					await existing.Upgrade(choiceContext);
				}
				// 免费征婚（HP不足）：只增加征婚值
				for (int i = effectiveAmount; i < amount; i++)
				{
					existing.IncrementMarryCountOnly();
				}
				// 征婚值变化后刷新图标显示（否则图标层数滞后与真实征婚值不一致）
				existingCreature.GetPower<SheHuaMarryCountPower>()?.RefreshDisplay();
				RefreshHitbox(existingCreature);
			}
			else
			{
				var creature = await PlayerCmd.AddPet<SheHuaXiaoJieMonster>(player);

				var sheHua = creature.Monster as SheHuaXiaoJieMonster;
				if (sheHua != null)
				{
					// 新建蛇花：至少给一次基础生命（否则 0 HP 会立即死亡），
					// 其余征婚只增加征婚值
					int baseUpgrades = System.Math.Max(effectiveAmount, 1);
					for (int i = 0; i < baseUpgrades; i++)
					{
						await sheHua.Upgrade(choiceContext);
					}
					// 其余征婚：只增加征婚值
					for (int i = baseUpgrades; i < amount; i++)
					{
						sheHua.IncrementMarryCountOnly();
					}

					// 强制显示血条
					var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
					if (creatureNode != null)
					{
						var stateDisplay = creatureNode.GetNode<NCreatureStateDisplay>("%HealthBar");
						stateDisplay?.AnimateIn(HealthBarAnimMode.SpawnedDuringCombat);
					}

					// 施加征婚层数显示能力（悬浮提示显示当前征婚层数；
					// 注意：宠物不是 Enemy，Creature.AfterAddedToRoom 不会触发 Monster.AfterAddedToRoom，所以在这里直接施加）
					if (!creature.HasPower<SheHuaMarryCountPower>())
					{
						await PowerCmd.Apply<SheHuaMarryCountPower>(new ThrowingPlayerChoiceContext(), creature, 1m, creature, null);
					}

					RefreshHitbox(creature);
				}
			}
		}
	}
}
