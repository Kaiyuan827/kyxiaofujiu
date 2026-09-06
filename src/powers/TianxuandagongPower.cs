using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;

namespace kyxiaofujiu.powers
{
	public sealed class TianxuandagongPower : PowerModel
	{
		private const int SalaryGain = 25;
		private const int HpCost = 1;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;

		// 玩家回合开始时触发
		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side != CombatSide.Player) return;
			if (Owner.IsDead) return;

			// 扣除1点生命
			await CreatureCmd.Damage(
				new ThrowingPlayerChoiceContext(),
				Owner,
				HpCost,
				ValueProp.Unblockable | ValueProp.Unpowered,
				null,
				null
			);

			if (Owner.IsDead) return;

			// 获得25工资
			SalaryCmd.Gain(Owner.Player, SalaryGain);
		}
	}
}
