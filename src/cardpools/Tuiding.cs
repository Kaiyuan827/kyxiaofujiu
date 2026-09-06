using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.commands;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.cardpools
{
    /// <summary>
    /// 退订 — 技能牌，打出所有手牌中的夫黑牌（从左到右），消耗
    /// </summary>
	public sealed class Tuiding : XiaofujiuCardBase
	{
		protected override int CanonicalEnergyCost => IsUpgraded ? 1 : 2;  // 费用 2/1

		public override FufuState CanonicalFufuState => FufuState.Black;
		public override bool HasBlackEffect => true;

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.AddRange(HoverTipHelper.FuheiPlayTips);
				return tips;
			}
		}

		public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
        {
            CardKeyword.Exhaust
        };

        public Tuiding()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            // 获取手牌中所有夫黑牌（手牌顺序即为从左到右）
            var hand = PileType.Hand.GetPile(Owner);
            var fufuBlackCards = hand.Cards
                .Where(c => c is XiaofujiuCardBase fc && fc.IsFufuBlack)
                .ToList();

            foreach (var card in fufuBlackCards)
            {
                // 从左到右依次强制打出夫黑牌
                await FufuBlackCardPlay.Play(card, choiceContext);
            }
            // 退订自身有消耗关键词，打出后自动进消耗堆
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}
