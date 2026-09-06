using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	public sealed class YayixingtaiPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;  // ✅ 改为 Counter

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;

		// 翻倍倍数 = 2 ^ Amount（2的层数次方）
		public int GetMultiplier()
		{
			return (int)System.Math.Pow(2, Amount);
		}
	}
}
