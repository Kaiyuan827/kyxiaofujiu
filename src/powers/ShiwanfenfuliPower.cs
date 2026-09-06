using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
    /// <summary>
    /// 十万粉福利能力 — 每回合开始选牌转化，参考 ToolsOfTheTradePower
    /// Amount 叠加：打出2张则每回合转化2张
    /// </summary>
    public sealed class ShiwanfenfuliPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.ConvertTips;

        // 每回合开始让玩家选择要转化的牌
        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != Owner.Player) return;
            if (Amount <= 0) return;

            var hand = PileType.Hand.GetPile(Owner.Player);
            if (hand == null || hand.Cards.Count == 0) return;

            // 过滤出夫黑夫白牌
            var fufuCards = hand.Cards
                .Where(c => c is XiaofujiuCardBase fc && fc.IsFufu && !fc.IsFufuYellow)
                .ToList();
            if (fufuCards.Count == 0) return;

            int toSelect = System.Math.Min(Amount, fufuCards.Count);

            // 可选转化：min=0（可跳过），max=toSelect（可少于最大数量）
            var selected = await CardSelectCmd.FromHand(
                choiceContext,
                Owner.Player,
                new CardSelectorPrefs(
                    new LocString("powers", "SHIWANFENFULI_POWER.selectPrompt"), 0, toSelect),
                c => c is XiaofujiuCardBase fc && fc.IsFufu && !fc.IsFufuYellow,
                this
            );

            foreach (var card in selected)
            {
                if (card is XiaofujiuCardBase fc)
                    FufuCmd.Toggle(fc);
            }
        }
    }
}
