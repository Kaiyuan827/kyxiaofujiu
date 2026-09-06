using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
	/// 天选打工人 — 攻击：1费，初始夫黑·固有。
	/// 造成 14 点伤害（升级 18），获得 20 工资。
	/// 当前工资每比 350 少 10，伤害与获得的工资各 +1（工资 ≥ 350 时无加成）。
	/// </summary>
	public sealed class Tianxuandagongren : XiaofujiuCardBase
	{
		// 初始夫黑（与打工人一致）
		public override FufuState CanonicalFufuState => FufuState.Black;

		// 固有：战斗开始时必定在手牌
		protected override IEnumerable<CardKeyword> GetBaseKeywords()
		{
			yield return CardKeyword.Innate;
		}

		public override bool ShowSalaryHover => true;

		// 动态伤害：基础 14 + 1 × 缺口级数（每少 10 工资 +1），CalculatedDamage 实时显示当前伤害
	// 动态工资：SalaryBase 20 + 1 × 缺口级数，SalaryGain(CalculatedVar) 实时显示当前获得工资
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new CalculationBaseVar(14m),
		new ExtraDamageVar(1m),
		new CalculatedDamageVar(ValueProp.Move)
			.WithMultiplier((CardModel card, Creature? _) => BonusLevels(card)),
		new IntVar("SalaryBase", 20m),
		new SalaryGainCalculatedVar()
			.WithMultiplier((CardModel card, Creature? _) => BonusLevels(card))
	};

	// 不出现在卡牌奖励中（先古卡牌）
		public override bool CanBeGeneratedInCombat => false;
		public override bool CanBeGeneratedByModifiers => false;

		public Tianxuandagongren()
			: base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
		{
		}

		// 工资缺口级数：当前工资每比 350 少 10 → 1 级（工资 ≥ 350 为 0）
		private static int BonusLevels(CardModel card)
		{
			if (card.Owner == null) return 0;
			var landlord = card.Owner.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
			if (landlord == null) return 0;
			int salary = landlord.Salary;
			if (salary >= 350) return 0;
			return (350 - salary) / 10;
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

			// 造成动态伤害（含缺口加成，走完整伤害 Hook）
			await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
				.FromCard(this, cardPlay)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 获得工资 = 20 + 缺口级数（统一入口，含爆米加成）
			int gain = 20 + BonusLevels(this);
			SalaryCmd.Gain(Owner, gain);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["CalculationBase"].UpgradeValueBy(4m); // 14 → 18（工资不成长）
		}

		/// <summary>
		/// 工资的动态计算变量：SalaryGain = SalaryBase(20) + ExtraDamage(1) × 缺口级数
		/// 用独立的 SalaryBase 槽（而非 CalculationBase，避免与伤害的基础 14 冲突）
		/// </summary>
		private sealed class SalaryGainCalculatedVar : CalculatedVar
		{
			public SalaryGainCalculatedVar() : base("SalaryGain") { }

			protected override DynamicVar GetBaseVar() => ((CardModel)_owner).DynamicVars["SalaryBase"];

			protected override DynamicVar GetExtraVar() => ((CardModel)_owner).DynamicVars.ExtraDamage;
		}
	}
}
