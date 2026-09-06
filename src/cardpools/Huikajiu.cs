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
    /// <summary>
    /// 辉卡九 — 手牌费用总和为9时打出，AOE后返回手牌
    /// </summary>
    public sealed class Huikajiu : XiaofujiuCardBase
    {
        public override FufuState CanonicalFufuState => FufuState.White;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(40m, ValueProp.Move)
        };

        public Huikajiu()
            : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
        {
        }

        // 检查手牌费用总和是否为9
        private bool IsHandCostNine()
        {
            if (Owner?.Piles == null) return false;
            var hand = PileType.Hand.GetPile(Owner);
            if (hand == null) return false;

            // 使用战斗中的“表面费用”（含夫化/能力药水/异蛇之眼/烟雾弥漫/带刺手甲等所有
            // 本卡 + 全局费用改动），并用 0 兜底，避免诅咒卡（基础费为 -1）和负费卡把总和往下拉。
            int totalCost = hand.Cards
                .Where(c => c.Id != base.Id)  // 排除此牌自身（在计算时此牌还在手牌中）
                .Sum(c => Math.Max(0, c.EnergyCost.GetWithModifiers(CostModifiers.All)));

            // 此牌自身同样按表面费用计入（若它被夫化/遗物等改了费，也应生效）
            totalCost += Math.Max(0, EnergyCost.GetWithModifiers(CostModifiers.All));

            return totalCost == 9;
        }

        // 不可打出时变灰（自动由 IsPlayable 控制）
        protected override bool IsPlayable
        {
            get
            {
                if (!base.IsPlayable) return false;
                return IsHandCostNine();
            }
        }

        // 参考 Silent 的华丽收场：满足可打出条件时，卡牌边缘闪金光
        protected override bool ShouldGlowGoldInternal => IsPlayable;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int totalDamage = IsUpgraded ? 50 : 40;

            await DamageCmd.Attack(totalDamage)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(Owner.Creature.CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(10m);
        }
    }
}
