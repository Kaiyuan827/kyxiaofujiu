using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace kyxiaofujiu.utils
{
	public static class HoverTipHelper
	{
		// ===== 征婚机制 =====
		private static readonly LocString _marriageTitle = new LocString("cards", "MARRIAGE_MECHANICS_TITLE");
		private static readonly LocString _marriageDesc = new LocString("cards", "MARRIAGE_MECHANICS_DESC");
		public static IEnumerable<IHoverTip> MarriageTips => new IHoverTip[] { new HoverTip(_marriageTitle, _marriageDesc) };

		// ===== 打工状态 =====
		private static readonly LocString _dagongTitle = new LocString("cards", "DAGONG_MECHANICS_TITLE");
		private static readonly LocString _dagongDesc = new LocString("cards", "DAGONG_MECHANICS_DESC");
		public static IEnumerable<IHoverTip> DagongTips => new IHoverTip[] { new HoverTip(_dagongTitle, _dagongDesc) };

		// ===== 完美格挡 =====
		private static readonly LocString _perfectBlockTitle = new LocString("cards", "PERFECT_BLOCK_MECHANICS_TITLE");
		private static readonly LocString _perfectBlockDesc = new LocString("cards", "PERFECT_BLOCK_MECHANICS_DESC");
		public static IEnumerable<IHoverTip> PerfectBlockTips => new IHoverTip[] { new HoverTip(_perfectBlockTitle, _perfectBlockDesc) };

		// ✅ ===== 转换机制 =====
		private static readonly LocString _convertTitle = new LocString("cards", "CONVERT_MECHANICS_TITLE");
		private static readonly LocString _convertDesc = new LocString("cards", "CONVERT_MECHANICS_DESC");
		public static IHoverTip ConvertTip => new HoverTip(_convertTitle, _convertDesc);
		public static IEnumerable<IHoverTip> ConvertTips => new IHoverTip[] { ConvertTip };
		
		// 在 HoverTipHelper 类中添加
private static readonly LocString _fuheiPlayTitle = new LocString("cards", "FUHEI_PLAY_MECHANICS_TITLE");
private static readonly LocString _fuheiPlayDesc = new LocString("cards", "FUHEI_PLAY_MECHANICS_DESC");
public static IHoverTip FuheiPlayTip => new HoverTip(_fuheiPlayTitle, _fuheiPlayDesc);
public static IEnumerable<IHoverTip> FuheiPlayTips => new IHoverTip[] { FuheiPlayTip };
	}
}
