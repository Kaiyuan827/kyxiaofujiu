using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 入夫门 — 攻击：造成 6 点伤害，本场战斗每次转化伤害+4（升级+5）
	/// </summary>
	public sealed class Rufumen : XiaofujiuCardBase
	{
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new CalculationBaseVar(6m),
			new ExtraDamageVar(4m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => XiaofujiuCardBase.GetCombatConvertCount(card.Owner))
		};

		public Rufumen()
			: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.ExtraDamage.UpgradeValueBy(1m);
		}

		// 进入战斗：重置本场转化计数（计数器由 XiaofujiuCardBase 转化时递增）
		public override Task AfterCardEnteredCombat(CardModel card)
		{
			if (card != this) return Task.CompletedTask;
			if (IsClone) return Task.CompletedTask;

			XiaofujiuCardBase.ResetCombatConvertCounts();
			return Task.CompletedTask;
		}
	}
}
