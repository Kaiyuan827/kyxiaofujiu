using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using kyxiaofujiu.core;

namespace kyxiaofujiu.commands
{
	public static class FufuBlackCardPlay
	{
		public static async Task Play(CardModel card, PlayerChoiceContext choiceContext, Creature? target = null)
		{
			if (card == null)
			{
				Log.Error("FufuBlackCardPlay: 卡牌为 null");
				return;
			}

			if (card is not XiaofujiuCardBase baseCard || !baseCard.IsFufuBlack)
			{
				Log.Error($"FufuBlackCardPlay: 卡牌 {card.Id} 不是夫黑状态");
				return;
			}

			if (card.Owner == null)
			{
				Log.Error($"FufuBlackCardPlay: 卡牌 {card.Id} 没有持有者");
				return;
			}

			var player = card.Owner;
			var combatState = player.Creature.CombatState;
			if (combatState == null)
			{
				Log.Error("FufuBlackCardPlay: 战斗状态为 null");
				return;
			}

			Creature? finalTarget = target;
			if (finalTarget == null)
			{
				finalTarget = DetermineTarget(card, combatState);
			}

			if (card.TargetType != TargetType.None && card.TargetType != TargetType.Self)
			{
				if (finalTarget == null)
				{
					Log.Error($"FufuBlackCardPlay: 卡牌 {card.Id} 需要目标但无法确定");
					return;
				}
				if (!finalTarget.IsAlive)
				{
					Log.Error($"FufuBlackCardPlay: 目标 {finalTarget.Name} 已死亡");
					return;
				}
			}

			// ✅ ResourceInfo 使用 default 或属性初始化
			var resources = default(ResourceInfo);

			Log.Info($"=== FufuBlackCardPlay: 强制打出夫黑卡牌 {card.Id}，目标: {finalTarget?.Name ?? "无"} ===");

			try
			{
				// X 费用夫黑卡被强制打出时不会走正常付费流程（PlayCardAction.SpendResources），
				// CapturedXValue 不会被赋值 → X=0 → 攻击 0 次 = 看起来"打不出"（哦牛批/征个未来）。
				// 白嫖处理：直接把 X 设为玩家当前能量，但不真正扣除能量（强制打出本身免费）。
				if (card.EnergyCost.CostsX)
				{
					card.EnergyCost.CapturedXValue = card.Owner?.PlayerCombatState?.Energy ?? 0;
				}

				await card.OnPlayWrapper(choiceContext, finalTarget, isAutoPlay: false, resources, skipCardPileVisuals: false);
			}
			catch (Exception ex)
			{
				Log.Error($"FufuBlackCardPlay: 打出夫黑卡牌 {card.Id} 失败: {ex.Message}");
				Log.Error(ex.StackTrace);
				throw;
			}
		}

		private static Creature? DetermineTarget(CardModel card, ICombatState combatState)
		{
			switch (card.TargetType)
			{
				case TargetType.Self:
					return card.Owner.Creature;

				case TargetType.None:
					return null;

				case TargetType.AnyEnemy:
					return combatState.Enemies.FirstOrDefault(e => e.IsAlive && e.IsHittable);

				case TargetType.AllEnemies:
                                        // AOE 攻击本身用 TargetingAllOpponents 不依赖该目标，但夫黑说话强制打出要求目标非空，
                                        // 返回第一个存活敌人作为占位目标，保证 AllEnemies 卡能被强制打出
                                        return combatState.Enemies.FirstOrDefault(e => e.IsAlive && e.IsHittable);
				case TargetType.AnyAlly:
					return combatState.PlayerCreatures
						.Where(c => c.IsAlive && c != card.Owner.Creature)
						.FirstOrDefault();

				case TargetType.RandomEnemy:
					var enemies = combatState.Enemies.Where(e => e.IsAlive && e.IsHittable).ToList();
					if (enemies.Count == 0) return null;
					// 多人安全：用卡主玩家的确定性 RNG 选目标
					if (card.Owner?.RunState?.Rng?.CombatCardSelection is { } rng)
					{
						return enemies[rng.NextInt(0, enemies.Count)];
					}
					return enemies[0];

				default:
					Log.Warn($"FufuBlackCardPlay: 未处理的 TargetType {card.TargetType}");
					return null;
			}
		}
	}
}
