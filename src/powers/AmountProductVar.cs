using System;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 一个“只用于显示”的动态变量：实时读取所属 power 的 Amount 并套用一个计算函数得到展示值。
	/// 例如火堆零点五用 {TotalHeal} 显示 "5 × Amount（层数）"，而 Amount 语义仍是层数、不参与实际计算。
	/// 仅用于渲染文本，不参与任何游戏状态计算。
	/// </summary>
	public sealed class AmountProductVar : DynamicVar
	{
		private readonly Func<PowerModel, int> _compute;

		public AmountProductVar(string name, Func<PowerModel, int> compute)
			: base(name, 0m)
		{
			_compute = compute;
		}

		protected override decimal GetBaseValueForIConvertible()
		{
			return _owner is PowerModel p ? _compute(p) : 0m;
		}

		public override string ToString()
		{
			return GetBaseValueForIConvertible().ToString();
		}
	}
}
