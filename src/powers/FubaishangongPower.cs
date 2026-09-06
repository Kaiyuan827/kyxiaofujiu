using System.Threading.Tasks;
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
	public sealed class FubaishangongPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		// 可叠加：Amount = 每斩杀获得的工资数（每张+20，升级+30）
		public override PowerStackType StackType => PowerStackType.Counter;

		public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
		{
			if (dealer != Owner) return;
			if (target.Side != CombatSide.Enemy) return;
			if (!result.WasTargetKilled) return;

			Flash();

			// 每斩杀获得 Amount 工资（可叠加）
			SalaryCmd.Gain(Owner.Player, Amount);
		}
	}
}
