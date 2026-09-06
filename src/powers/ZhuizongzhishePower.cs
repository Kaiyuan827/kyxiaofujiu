using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 追踪之蛇能力 — 蛇花小姐可以对所有敌人造成伤害
	/// </summary>
	public sealed class ZhuizongzhishePower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		// 开关型（不叠加）：蛇花小姐对所有敌人造成伤害
		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;
	}
}
