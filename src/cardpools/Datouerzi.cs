using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class Datouerzi : XiaofujiuCardBase
    {
        public override bool HasBlackEffect => true;
        public override FufuState CanonicalFufuState => FufuState.White;

        // 完美打击式动态伤害：总伤害 = 基础 6 + 手牌夫黑数 × 8（升级 10）
        // CalculatedDamageVar 实时显示总值（6 + 8×黑数），随手中夫黑数量变化自动刷新（含高亮）
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new CalculationBaseVar(6m),
            new ExtraDamageVar(8m),
            new CalculatedDamageVar(ValueProp.Move)
                .WithMultiplier((CardModel card, Creature? _) => CountFufuBlackInHand(card))
        };

        public Datouerzi()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        // 静态（WithMultiplier 要求）：统计卡持有者手中的夫黑数量
        private static int CountFufuBlackInHand(CardModel card)
        {
            if (card.Owner == null) return 0;
            var hand = PileType.Hand.GetPile(card.Owner);
            if (hand == null) return 0;
            return hand.Cards.Count(c => c is XiaofujiuCardBase fc && fc.IsFufuBlack);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            // 动态伤害 = 基础6 + 手牌夫黑 × 每张加成（运行时自动计算，走完整伤害 Hook）
            await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.ExtraDamage.UpgradeValueBy(2m); // 每张夫黑 8 → 10
        }
    }
}
