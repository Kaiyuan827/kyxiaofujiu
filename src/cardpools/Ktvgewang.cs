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
    public sealed class Ktvgewang : XiaofujiuCardBase
    {
        public override bool HasBlackEffect => true;
        public override bool HasConvertEffect => true;
        public override bool HasMarriageEffect => true;
        public override FufuState CanonicalFufuState => FufuState.White;

        protected override IEnumerable<DynamicVar> CanonicalVars => new[]
        {
            new IntVar("MarryCount", 2m)  // 平衡性调整：征婚3→2，升级3
        };

        public Ktvgewang()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var selected = await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(new LocString("cards", "KTVGEWANG.selectPrompt"), 1),
                c => c is XiaofujiuCardBase fc && fc.IsFufuBlack,
                this
            );

            var card = selected.FirstOrDefault();
            // 转化一张夫黑（夫黑→夫白，卡保留在手牌）成功后征婚（平衡性调整：丢弃→转化）
            if (card != null)
            {
                FufuCmd.Toggle(card);

                int marryCount = IsUpgraded ? 3 : 2;
                await ZhengHunCmd.Marry(choiceContext, Owner, marryCount);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["MarryCount"].UpgradeValueBy(1m);
        }
    }
}
