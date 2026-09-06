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
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
    public sealed class Caitoudazhan : XiaofujiuCardBase
    {
        public override bool HasBlackEffect => true;
        public override FufuState CanonicalFufuState => FufuState.White;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(10m, ValueProp.Move)
        };

        public Caitoudazhan()
            : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        private bool IsHandBalanced()
        {
            if (Owner?.Piles == null) return false;
            var hand = PileType.Hand.GetPile(Owner);
            if (hand == null) return false;

            int black = 0, white = 0;
            foreach (var c in hand.Cards)
            {
                if (c.Id == base.Id) continue;
                if (c is XiaofujiuCardBase fc)
                {
                    if (fc.IsFufuBlack) black++;
                    else if (fc.IsFufuWhite) white++;
                }
            }
            return black == white && black > 0;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            bool attacksTwice = IsHandBalanced();

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			if (attacksTwice)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
					.FromCard(this, cardPlay)
					.Targeting(cardPlay.Target)
					.WithHitFx("vfx/vfx_attack_slash")
					.Execute(choiceContext);
			}
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }
}
