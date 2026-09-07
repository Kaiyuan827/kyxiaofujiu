using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.monsters
{
	/// <summary>
	/// 蛇花小姐 — 征婚召唤物（奥斯提式伤害吸收）
	/// - HP = 征婚值 × 2
	/// - 回合结束造成 3 次伤害，每次 = 征婚值
	/// - 玩家先扣格挡，未格挡伤害重定向到蛇花小姐（超过蛇花小姐 HP 的溢出伤害返回玩家）
	/// - 战斗结束：晓夫九恢复蛇花小姐当前HP
	/// </summary>
	public sealed class SheHuaXiaoJieMonster : MonsterModel
	{
		private int _marryCount;

		public int MarryCount => _marryCount;

		public SheHuaXiaoJieMonster() { }

		public override int MinInitialHp => 0;
		public override int MaxInitialHp => 0;

		public override bool IsHealthBarVisible => Creature.IsAlive;

		protected override string VisualsPath => "res://scenes/creature_visuals/she_hua_xiao_jie.tscn";

		protected override MonsterMoveStateMachine GenerateMoveStateMachine()
		{
			var state = new MoveState("SHE_HUA_IDLE", DoNothing, new SingleAttackIntent(0));
			state.FollowUpState = state;
			return new MonsterMoveStateMachine(new List<MonsterState> { state }, state);
		}

		private Task DoNothing(IReadOnlyList<Creature> targets) => Task.CompletedTask;

		// ==================== 回合结束攻击：3次×征婚值 ====================

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side != CombatSide.Player) return;
			if (Creature.IsDead || _marryCount <= 0) return;

			// 追踪之蛇：蛇花小姐对所有敌人造成伤害（AOE）
			bool aoe = Creature.PetOwner?.Creature.HasPower<ZhuizongzhishePower>() == true;

			// 伤害 = 征婚值 + 当前力量（力量可为负，如被"用萎靡"减力量；最低0）
			int strength = Creature.GetPower<StrengthPower>()?.Amount ?? 0;
			int damage = System.Math.Max(0, _marryCount + strength);

			for (int i = 0; i < 3; i++)
			{
				// 每段重算存活/可命中目标：前一段打死一个敌人后，后一段不再打已死目标，
				// 避免"群怪中杀死一个 -> 后续段空放"（目标列表不能用循环外快照）。
				var enemies = CombatState.Creatures
					.Where(c => c.Side == CombatSide.Enemy && c.IsAlive && c.IsHittable)
					.ToList();
				if (enemies.Count == 0) break;

				bool killed = false;
				if (aoe)
				{
					foreach (var target in enemies)
					{
						// Move | Unpowered：保留攻击语义（可格挡），但不被识别为"强力攻击"，
						// 荆棘（ThornsPower）的 IsPoweredAttack() 判定为 false，因此不会被反弹
						var rs = await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move | ValueProp.Unpowered, Creature, null, null);
						killed |= rs.Any(r => r.WasTargetKilled);
					}
				}
				else
				{
					// 多人安全：用主人的确定性 RNG 选目标（GD.Randi 双端不一致会导致动作不同步）
					var petOwner = Creature.PetOwner;
					var target = enemies[0];
					if (petOwner?.RunState?.Rng?.CombatCardSelection is { } rng)
					{
						target = enemies[rng.NextInt(0, enemies.Count)];
					}
					var rs = await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Move | ValueProp.Unpowered, Creature, null, null);
					killed |= rs.Any(r => r.WasTargetKilled);
				}

				// 本段击杀（如异蛙寄生虫被炸死并召唤小寄生虫）后，给"死亡动画 + 召唤"留出时间，
				// 让"第一击→死亡爆炸→小寄生虫出现→再打第二/三击"有节奏，避免三段连续打出显得一次性放完。
				if (i < 2)
				{
					if (killed)
						await Cmd.CustomScaledWait(0.4f, 0.65f);
					else
						await Cmd.CustomScaledWait(0.15f, 0.25f);
				}
			}
		}

		// ==================== 奥斯提式伤害重定向 ====================

		public override Creature ModifyUnblockedDamageTarget(Creature originalTarget, decimal amount, ValueProp props, Creature? dealer)
		{
			var petOwner = Creature.PetOwner;
			if (petOwner == null) return originalTarget;
			if (originalTarget != petOwner.Creature) return originalTarget;
			if (Creature.IsDead) return originalTarget;

			// 不为你而死：蛇花小姐不再为玩家承受伤害
			if (petOwner.Creature.HasPower<BuweinierersiPower>()) return originalTarget;

			// 不拦截自伤（征婚扣血等 dealer==null 或 Unpowered 标记的伤害）
			if (dealer == null) return originalTarget;
			if (props.HasFlag(ValueProp.Unpowered)) return originalTarget;

			// 将未格挡伤害重定向到蛇花小姐
			// 若伤害超过蛇花小姐 HP，溢出部分自动返回玩家（游戏原生机制）
			return Creature;
		}

		// ==================== 战斗结束恢复 ====================

		public override async Task AfterCombatVictory(CombatRoom room)
		{
			var owner = Creature.PetOwner;
			if (owner == null) return;

			int healAmount = Creature.CurrentHp;
			if (healAmount > 0)
				await CreatureCmd.Heal(owner.Creature, healAmount);
		}

		// ==================== 征婚强化 ====================

		public async Task Upgrade(PlayerChoiceContext choiceContext)
		{
			_marryCount++;
			// GainMaxHp 内部会自动回复等量血（加2最大生命即回2血），无需再显式 Heal
			await CreatureCmd.GainMaxHp(Creature, 2);
		}

		/// <summary>
		/// 仅增加征婚值，不增加生命（用于玩家HP为1无法支付征婚代价时）
		/// </summary>
		public void IncrementMarryCountOnly()
		{
			_marryCount++;
		}
	}
}
