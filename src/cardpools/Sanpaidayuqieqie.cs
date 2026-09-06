using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
    /// <summary>
    /// 三牌大于一切 — 能力牌，回合结束时手牌夫黑=3则下回合多抽1
    /// </summary>
    public sealed class Sanpaidayuqieqie : XiaofujiuCardBase
    {
        public override bool HasBlackEffect => true;
        protected override int CanonicalEnergyCost => IsUpgraded ? 0 : 1;

        public override FufuState CanonicalFufuState => FufuState.White;

        public Sanpaidayuqieqie()
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
            // 下回合多抽 2 张牌
            await PowerCmd.Apply<SanpaidayuqieqiePower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}
