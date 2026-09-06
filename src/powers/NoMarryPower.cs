using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 禁止征婚的负面效果，持续2回合
	/// </summary>
	public sealed class NoMarryPower : PowerModel
	{
		public override PowerType Type => PowerType.Debuff;
		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;

		/// <summary>
		/// 当征婚被本能力禁止时触发闪烁提示。
		/// 对齐官方“战斗专注/不可抽牌”的反馈：尝试抽牌被挡住时，能力图标会闪烁。
		/// </summary>
		public void NotifyBlocked()
		{
			Flash();
		}

		// 玩家回合结束时层数-1
		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side == CombatSide.Player)
			{
				await PowerCmd.TickDownDuration(this);
			}
		}
	}
}
