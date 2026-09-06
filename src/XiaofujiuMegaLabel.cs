using MegaCrit.Sts2.addons.mega_text;

namespace kyxiaofujiu.ui
{
	/// <summary>
	/// MegaLabel 的模组侧占位子类（教程推荐做法）：
	/// 场景中的 Label 挂本脚本而非跨包引用基础游戏 addons/mega_text/MegaLabel.cs，
	/// 避免跨包脚本 UID 解析失败（Cannot get class ''）。
	/// 注意：场景里的 [Export] 属性（MinFontSize/MaxFontSize）在导出 pck 时会被剥离
	/// （C# 导出属性不序列化），所以必须在 _Ready 里显式设置字号范围，再调用
	/// base._Ready() 让 MegaLabel 按此范围自动缩放（文本不超界时取 MaxFontSize）。
	/// </summary>
	public partial class XiaofujiuMegaLabel : MegaLabel
	{
		public override void _Ready()
		{
			MinFontSize = 26;
			MaxFontSize = 30;
			base._Ready();
		}
	}
}
