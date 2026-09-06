using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 不为你而死能力 — 征婚不再消耗生命，蛇花小姐也不再为玩家承受伤害
	/// </summary>
	public sealed class BuweinierersiPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		// 开关型（不叠加）：征婚免费 + 蛇花不承受伤害，打多张不叠加
		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;
	}
}
