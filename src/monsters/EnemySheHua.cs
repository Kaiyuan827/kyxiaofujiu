using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace kyxiaofujiu.monsters
{
	/// <summary>
	/// 敌方蛇花 — 白区召唤的爪牙
	/// 固定意图：造成(7/8)*3点伤害
	/// </summary>
	public sealed class EnemySheHua : MonsterModel
	{
		// 低进阶 / 高进阶
		public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 35);
		public override int MaxInitialHp => MinInitialHp;

		private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

		public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

		protected override string VisualsPath => "res://scenes/creature_visuals/di_fang_she_hua.tscn";

		protected override MonsterMoveStateMachine GenerateMoveStateMachine()
		{
			var attack = new MoveState("MULTI_ATTACK_MOVE", AttackMove, new MultiAttackIntent(Damage, 3));
			attack.FollowUpState = attack;

			return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
		}

		private async Task AttackMove(IReadOnlyList<Creature> targets)
		{
			await DamageCmd.Attack(Damage)
				.WithHitCount(3)
				.FromMonster(this)
				.WithAttackerAnim("Attack", 0.3f)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(null);
		}
	}
}
