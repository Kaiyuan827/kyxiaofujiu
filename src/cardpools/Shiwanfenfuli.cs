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
    /// 十万粉福利 — 能力牌
    /// 夫黑：需夫黑说话打出
    /// 升级后初始夫白，可直接打出
    /// </summary>
    public sealed class Shiwanfenfuli : XiaofujiuCardBase
    {
        public override bool HasConvertEffect => true;
        public override FufuState CanonicalFufuState => FufuState.Black;

        public Shiwanfenfuli()
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override void AfterCloned()
        {
            base.AfterCloned();
            // 升级后初始为夫白（可正常打出）
            if (IsUpgraded && IsFufuBlack)
                Unseal();
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
            await PowerCmd.Apply<ShiwanfenfuliPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
        }
    }
}
