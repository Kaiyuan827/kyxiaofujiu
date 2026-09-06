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
	public sealed class Fugedajianbing : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.Black;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new DamageVar(12m, ValueProp.Move),
			new IntVar("DrawCount", 1m)
		};

		public Fugedajianbing()
			: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
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

			int drawCount = IsUpgraded ? 2 : 1;
			await CardPileCmd.Draw(choiceContext, drawCount, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2m);
			DynamicVars["DrawCount"].UpgradeValueBy(1m);
		}
	}
}
