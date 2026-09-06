using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
    /// <summary>
    /// 苦命鸳鸯 — 获得抽牌堆中的随机一张夫黑和夫白（重做），消耗
    /// </summary>
    public sealed class Kumingyuanyang : XiaofujiuCardBase
    {
        public override bool HasBlackEffect => true;
        public override bool HasConvertEffect => false;
        public override FufuState CanonicalFufuState => FufuState.White;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
        {
            CardKeyword.Exhaust
        };

        public Kumingyuanyang()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.None)  // 重做：1费
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            // 重做：从抽牌堆随机取一张夫黑和一张夫白加入手牌
            var drawPile = PileType.Draw.GetPile(Owner);
            if (drawPile == null) return;

            var rng = Owner.RunState.Rng.CombatCardSelection;
            var blackCards = drawPile.Cards.OfType<XiaofujiuCardBase>().Where(c => c.IsFufuBlack).ToList();
            var whiteCards = drawPile.Cards.OfType<XiaofujiuCardBase>().Where(c => c.IsFufuWhite).ToList();

            var black = rng.NextItem(blackCards);
            if (black != null)
                await CardPileCmd.Add(black, PileType.Hand);

            var white = rng.NextItem(whiteCards);
            if (white != null)
                await CardPileCmd.Add(white, PileType.Hand);
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }
}
