using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.cardpools;

namespace kyxiaofujiu.powers
{
	public sealed class BiedaraofugePower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
{
	if (player == Owner.Player)
	{
		Flash();
		// 每层生成1张加班
		for (int i = 0; i < Amount; i++)
		{
			await Jiaban.CreateInHand(Owner.Player, combatState);
		}
	}
}
	}
}
