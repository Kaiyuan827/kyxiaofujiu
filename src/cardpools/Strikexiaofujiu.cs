using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Strikexiaofujiu : XiaofujiuCardBase
	{
		protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new DamageVar(6m, ValueProp.Move)
		};

		// ✅ 不出现在卡牌奖励中
		public override bool CanBeGeneratedInCombat => false;
		public override bool CanBeGeneratedByModifiers => false;

		public Strikexiaofujiu()
			: base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
