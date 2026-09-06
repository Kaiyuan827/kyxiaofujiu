using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 打工人 — 攻击（初始夫黑·固有）：造成伤害并获得工资（幸运房东）
	/// 重做：继承原哦牛批的夫黑/固有/数值，造成 14 点伤害并获 20 工资；升级伤害 19。
	/// </summary>
	public sealed class Dagongren : XiaofujiuCardBase
	{
		// 初始夫黑（封印态，不可直接打出，需夫黑说话/转化解除）
		public override FufuState CanonicalFufuState => FufuState.Black;

		// 固有词条：战斗开始时必定出现在手牌中
		protected override IEnumerable<CardKeyword> GetBaseKeywords()
		{
			yield return CardKeyword.Innate;
		}

		public override bool ShowSalaryHover => true;
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new DamageVar(14m, ValueProp.Move),
			new IntVar("SalaryGain", 20m)
		};

		public override bool CanBeGeneratedInCombat => false;
		public override bool CanBeGeneratedByModifiers => false;

		public Dagongren()
			: base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

			// 造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 获得工资（统一入口，含爆米加成）
			SalaryCmd.Gain(Owner, (int)DynamicVars["SalaryGain"].BaseValue);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(5m); // 14 → 19（工资固定 20，不成长）
		}
	}
}
