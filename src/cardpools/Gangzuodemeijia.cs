using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
    /// <summary>
    /// 刚做的美甲 — 转化所有手牌
    /// </summary>
    public sealed class Gangzuodemeijia : XiaofujiuCardBase
    {
        protected override int CanonicalEnergyCost => IsUpgraded ? 0 : 1;

        public override bool HasConvertEffect => true;
        public override FufuState CanonicalFufuState => FufuState.Black;

        public Gangzuodemeijia()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var hand = PileType.Hand.GetPile(Owner);
            if (hand == null) return;

            // 快照手牌再遍历：转化会触发“一次一个”等异步抽牌，活集合枚举会抛
            // Collection was modified（刚做的美甲+一次一个配合曾卡死）
            foreach (var card in hand.Cards.ToList())
            {
                if (card is XiaofujiuCardBase fc && fc.IsFufu)
                    FufuCmd.Toggle(fc);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}
