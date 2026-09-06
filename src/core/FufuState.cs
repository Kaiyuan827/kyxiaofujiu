namespace kyxiaofujiu.core
{
	/// <summary>
	/// 夫黑夫白状态枚举
	/// </summary>
	public enum FufuState
	{
		/// <summary>无夫黑夫白属性</summary>
		None,
		/// <summary>夫白 - 纯净态，无特殊效果</summary>
		White,
		/// <summary>夫黑 - 封印态，获得"保留"和"不可打出"</summary>
		Black,
		/// <summary>夫黄 - 由夫黑/夫白转化而来，获得"保留"和"重放1"，不可再被转化</summary>
		Yellow
	}
}
