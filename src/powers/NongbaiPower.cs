using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.cardpools;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 农白的临时力量 — 给敌人施加本回合有效的力量（回合结束自动消失）。
	/// 参考游戏 PiercingWailPower（TemporaryStrengthPower 基类：BeforeApplied 加力量，
	/// AfterSideTurnEnd 回合结束撤销）。
	/// </summary>
	public sealed class NongbaiPower : TemporaryStrengthPower
	{
		public override AbstractModel OriginModel => ModelDb.Card<Nongbai>();

		protected override bool IsPositive => true;  // 正向力量（敌人力量+4）
	}
}
