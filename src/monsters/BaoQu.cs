using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace kyxiaofujiu.monsters
{
	/// <summary>
	/// 白区 — 三层Boss
	/// 能力「sl百连」：血量低于50时恢复满血，阈值-10，并召唤一只敌方蛇花
	/// 意图循环：意图1(防御+力量) → 意图2(多段+易伤虚弱) → 意图3(重击) → 循环
	/// </summary>
	public sealed class BaoQu : MonsterModel
	{
		private const int _blockGain = 30;
		private const int _strengthGain = 2;
		private const int _weakAmount = 2;
		private const int _vulnerableAmount = 2;

		// 低进阶 / 高进阶
		public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 130, 120);
		public override int MaxInitialHp => MinInitialHp;

		private int MultiDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
		private int BigDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 15);

		public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

		protected override string VisualsPath => "res://scenes/creature_visuals/bao_qv.tscn";

		// 让战斗预载敌方蛇花的资源
		public override IEnumerable<string> AssetPaths => base.AssetPaths.Concat(ModelDb.Monster<EnemySheHua>().AssetPaths);

		protected override MonsterMoveStateMachine GenerateMoveStateMachine()
		{
			var states = new List<MonsterState>();

			// 意图1：获得30格挡，力量+2
			var move1 = new MoveState("BUFF_DEFEND_MOVE", BuffDefendMove, new DefendIntent(), new BuffIntent());
			// 意图2：造成(7/8)*3伤害，给予2层虚弱和2层易伤
			var move2 = new MoveState("MULTI_DEBUFF_MOVE", MultiDebuffMove, new MultiAttackIntent(MultiDamage, 3), new DebuffIntent());
			// 意图3：造成15/18伤害
			var move3 = new MoveState("BIG_ATTACK_MOVE", BigAttackMove, new SingleAttackIntent(BigDamage));

			move1.FollowUpState = move2;
			move2.FollowUpState = move3;
			move3.FollowUpState = move1;

			states.Add(move1);
			states.Add(move2);
			states.Add(move3);

			return new MonsterMoveStateMachine(states, move1);
		}

		// 意图1：30格挡 + 力量+2
		private async Task BuffDefendMove(IReadOnlyList<Creature> targets)
		{
			await CreatureCmd.GainBlock(Creature, _blockGain, ValueProp.Move, null);
			await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, _strengthGain, Creature, null);
		}

		// 意图2：3段伤害 + 2层虚弱 + 2层易伤
		private async Task MultiDebuffMove(IReadOnlyList<Creature> targets)
		{
			await DamageCmd.Attack(MultiDamage)
				.WithHitCount(3)
				.FromMonster(this)
				.WithAttackerAnim("Attack", 0.4f)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(null);

			await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, _weakAmount, Creature, null);
			await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, _vulnerableAmount, Creature, null);
		}

		// 意图3：重击
		private async Task BigAttackMove(IReadOnlyList<Creature> targets)
		{
			await DamageCmd.Attack(BigDamage)
				.FromMonster(this)
				.WithAttackerAnim("Attack", 0.5f)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(null);
		}

		// 战斗开始时施加「sl百连」能力（血量低于阈值时回满血、阈值-10、召唤敌方蛇花）
		public override async Task AfterAddedToRoom()
		{
			await base.AfterAddedToRoom();
			await PowerCmd.Apply<SlBaiLianPower>(new ThrowingPlayerChoiceContext(), Creature, 50m, Creature, null);
		}
	}
}
