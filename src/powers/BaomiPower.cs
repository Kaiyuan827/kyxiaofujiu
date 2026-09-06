using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace kyxiaofujiu.powers
{
	public sealed class BaomiPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;  // 支持叠加

		// Amount 语义：每次获得工资时额外获得的工资（由 Baomi 卡按 SalaryGain 施加，叠加时累加）。
		// 获得工资时由 SalaryCmd.Gain 统一处理：额外增加 Amount 工资。
	}
}
