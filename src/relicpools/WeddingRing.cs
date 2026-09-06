using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 婚戒 — 征婚50次后变为征婚戒指
	/// </summary>
	public sealed class WeddingRing : RelicModel
	{
		private int _marryCount;

		public override RelicRarity Rarity => RelicRarity.Event;

		public override bool ShowCounter => true;
		public override int DisplayAmount => _marryCount;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;

		[SavedProperty]
		public int MarryCount
		{
			get => _marryCount;
			set
			{
				AssertMutable();
				_marryCount = value;
				InvokeDisplayAmountChanged();
			}
		}

		/// <summary>
		/// 由 ZhengHunCmd 补丁调用，累计征婚次数。
		/// 达到 50 次后自动替换为征婚戒指。
		/// </summary>
		public async Task IncrementMarry(int amount)
		{
			if (amount <= 0) return;
			MarryCount += amount;
			Flash();
			if (MarryCount >= 50)
			{
				await RelicCmd.Replace(this, ModelDb.Relic<ProposalRing>().ToMutable());
			}
		}
	}
}
