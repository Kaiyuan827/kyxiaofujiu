using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.monsters;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 勒紧我能力 — 本回合蛇花小姐协助攻击：每打出攻击牌，额外造成一段等于征婚值的伤害
	/// </summary>
	public sealed class LejinwoPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 只响应玩家打出的攻击牌
			if (cardPlay.Card.Type != CardType.Attack) return;
			if (Owner == null || Owner.IsDead) return;
			if (cardPlay.Player != Owner.Player) return;

			var combatState = Owner.CombatState;
			if (combatState == null) return;

			// 找到存活的我方蛇花小姐
			var sheHuaCreature = combatState.Allies
				.FirstOrDefault(c => c.Monster is SheHuaXiaoJieMonster && c.PetOwner == Owner.Player && c.IsAlive);
			if (sheHuaCreature?.Monster is not SheHuaXiaoJieMonster sheHua) return;
			if (sheHua.MarryCount <= 0) return;

			Flash();

			int strength = sheHuaCreature.GetPower<StrengthPower>()?.Amount ?? 0;
			int damage = System.Math.Max(0, sheHua.MarryCount + strength);

			// 追踪之蛇：蛇花小姐对所有存活敌人各造成一段伤害（AOE）
			bool aoe = Owner.HasPower<ZhuizongzhishePower>();
			if (aoe)
			{
				var enemies = combatState.Creatures
					.Where(c => c.Side == CombatSide.Enemy && c.IsAlive && c.IsHittable)
					.ToList();
				if (enemies.Count == 0) return;

				foreach (var enemy in enemies)
				{
					// Move | Unpowered：保留攻击语义但避免荆棘反弹（与蛇花回合结束 AOE 一致）
					await CreatureCmd.Damage(choiceContext, enemy, damage, ValueProp.Move | ValueProp.Unpowered, sheHuaCreature, null, null);
				}
				return;
			}

			// 单目标：优先攻击牌的目标；无目标/目标失效时随机选一个敌人
			var target = cardPlay.Target;
			if (target == null || !target.IsAlive || target.Side == CombatSide.Player)
			{
				var enemies = combatState.Creatures
					.Where(c => c.Side == CombatSide.Enemy && c.IsAlive && c.IsHittable)
					.ToList();
				if (enemies.Count == 0) return;
				// 多人安全：用主人的确定性 RNG 选目标
				target = enemies[0];
				if (Owner.Player?.RunState?.Rng?.CombatCardSelection is { } rng)
				{
					target = enemies[rng.NextInt(0, enemies.Count)];
				}
			}

			await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move | ValueProp.Unpowered, sheHuaCreature, null, null);
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			// 本回合结束后移除
			if (participants.Contains(Owner))
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}
