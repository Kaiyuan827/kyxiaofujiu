using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 欧金金 — 攻击：扣除10%工资，造成等量伤害（最高45，升级60）。
	/// 工资花费是动态值，不走固定 SalaryCost（所以不显示工资图标）。
	/// </summary>
	public sealed class Oujinjin : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;
		public override bool ShowSalaryHover => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MaxDamage", 45m)
		};

		public Oujinjin()
			: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

			// 扣除当前工资的 10%（四舍五入）
			var landlord = Owner.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
			int salary = landlord?.Salary ?? 0;
			int cost = (int)Math.Round(salary * 0.1);
			if (cost > 0 && landlord != null)
			{
				landlord.Salary -= cost;
			}

			// 造成等量伤害（最高 MaxDamage）
			int maxDamage = (int)DynamicVars["MaxDamage"].BaseValue;
			int damage = Math.Min(cost, maxDamage);

			await DamageCmd.Attack(damage)
				.FromCard(this, cardPlay)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["MaxDamage"].UpgradeValueBy(15m); // 45 → 60
		}
	}
}
