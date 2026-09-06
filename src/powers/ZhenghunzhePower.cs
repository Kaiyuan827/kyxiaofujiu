using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	public sealed class ZhenghunzhePower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;  // ✅ 改为 Counter

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;

		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side != CombatSide.Player) return;
			if (Owner.IsDead) return;

			// 每层征婚1次
			for (int i = 0; i < Amount; i++)
			{
				await ZhengHunCmd.Marry(new ThrowingPlayerChoiceContext(), Owner.Player, 1);
			}
		}
	}
}
