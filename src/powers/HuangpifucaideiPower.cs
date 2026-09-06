using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 黄皮夫才对 — 夫黄获得额外重放1
	/// </summary>
	public sealed class HuangpifucaideiPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		// Counter：可叠加，每张黄皮夫才对 Amount+1，夫黄额外重放 = Amount
		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
		{
			HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
		};

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			var player = Owner?.Player ?? applier?.Player;
			XiaofujiuCardBase.RefreshReplayCounts(player);
			return Task.CompletedTask;
		}

		public override Task AfterRemoved(Creature oldOwner)
		{
			XiaofujiuCardBase.RefreshReplayCounts(oldOwner.Player);
			return Task.CompletedTask;
		}
	}
}
