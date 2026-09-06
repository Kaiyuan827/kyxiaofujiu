using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
    /// <summary>
    /// 创世夫柱 — 能力牌，每次转换获得格挡
    /// </summary>
    public sealed class Chuangshifuzhu : XiaofujiuCardBase
    {
        public override bool HasConvertEffect => true;
        public override FufuState CanonicalFufuState => FufuState.White;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new IntVar("BlockGain", 4m)
        };

        public Chuangshifuzhu()
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
            int blockPerToggle = DynamicVars["BlockGain"].IntValue;
            await PowerCmd.Apply<ChuangshifuzhuPower>(choiceContext, Owner.Creature, blockPerToggle, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["BlockGain"].UpgradeValueBy(1m);
        }
    }
}
