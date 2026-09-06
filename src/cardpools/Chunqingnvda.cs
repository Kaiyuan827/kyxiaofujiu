using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
    /// <summary>
    /// 纯情女大 — 转化任意张夫白，每张提供征婚
    /// </summary>
    public sealed class Chunqingnvda : XiaofujiuCardBase
    {
        public override bool HasMarriageEffect => true;
        public override bool HasConvertEffect => true;
        public override FufuState CanonicalFufuState => FufuState.White;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new IntVar("MarryCount", 1m)
        };

        public Chunqingnvda()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var hand = PileType.Hand.GetPile(Owner);
            int maxSelect = hand?.Cards.Count ?? 0;

            var selected = await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(
                    new LocString("cards", "CHUNQINGNVDA.selectPrompt"),
                    0,
                    maxSelect),
                c => c is XiaofujiuCardBase fc && fc.IsFufuWhite,
                this
            );

            if (!selected.Any()) return;

            int marryPerCard = DynamicVars["MarryCount"].IntValue;
            int totalMarry = 0;

            foreach (var card in selected)
            {
                if (card is XiaofujiuCardBase fc)
                {
                    FufuCmd.Toggle(fc);
                    totalMarry += marryPerCard;
                }
            }

            if (totalMarry > 0)
                await ZhengHunCmd.Marry(choiceContext, Owner, totalMarry);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["MarryCount"].UpgradeValueBy(1m);
        }
    }
}
