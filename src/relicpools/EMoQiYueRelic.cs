using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 恶魔契约 — 若你付不起房租，则取消收租1次（只生效一次）。
	/// 由幸运房东 CollectRent 付不起分支调用 TryConsumeSkip 判断。
	/// </summary>
	public sealed class EMoQiYueRelic : RelicModel
	{
		private bool _used;

		public override RelicRarity Rarity => RelicRarity.Rare;

		protected override IEnumerable<DynamicVar> CanonicalVars => System.Array.Empty<DynamicVar>();

		// 是否已用掉（SL 持久化，防重复触发）
		[SavedProperty]
		public bool Used
		{
			get => _used;
			set { AssertMutable(); _used = value; }
		}

		/// <summary>
		/// 尝试消耗一次"取消收租"（由幸运房东收租时调用）；已用过或没有该遗物则返回 false
		/// </summary>
		public static bool TryConsumeSkip(Player? player)
		{
			var relic = player?.Relics.OfType<EMoQiYueRelic>().FirstOrDefault();
			if (relic == null || relic.Used) return false;
			relic.Used = true;
			return true;
		}
	}
}
