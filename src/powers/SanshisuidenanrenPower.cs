using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 三十岁的男人 — 当你征婚时，获得 2 倍征婚值的格挡（每层 2 倍，可叠加）。
	/// 逻辑在 ZhengHunCmd.Marry 中检查本能力（与压抑形态 YayixingtaiPower 同模式）。
	/// </summary>
	public sealed class SanshisuidenanrenPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;  // 可叠加：2张=4倍，3张=6倍

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				foreach (var tip in HoverTipHelper.MarriageTips) yield return tip;
				yield return HoverTipFactory.Static(StaticHoverTip.Block);
			}
		}
	}
}
