using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.relicpools
{
    public sealed class MyCustomRelic : RelicModel
    {
        private bool _hasBlockedAttackThisTurn;
        private bool _hasTakenDamageThisTurn;
        private int _perfectBlockCount;

        // 完美格挡机制悬浮说明
        protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.PerfectBlockTips;

        // 本回合已格挡/已受伤标记（SL 持久化，防 SL 回战斗中丢失导致完美格挡重复计数）
        [SavedProperty]
        public bool HasBlockedAttackThisTurn
        {
            get => _hasBlockedAttackThisTurn;
            set { AssertMutable(); _hasBlockedAttackThisTurn = value; }
        }

        [SavedProperty]
        public bool HasTakenDamageThisTurn
        {
            get => _hasTakenDamageThisTurn;
            set { AssertMutable(); _hasTakenDamageThisTurn = value; }
        }

        public override RelicRarity Rarity => RelicRarity.Event;

        public override bool ShowCounter => true;
        public override int DisplayAmount => _perfectBlockCount;

        [SavedProperty]
        public int PerfectBlockCount
        {
            get => _perfectBlockCount;
            set
            {
                AssertMutable();
                _perfectBlockCount = value;
                InvokeDisplayAmountChanged();
            }
        }

        public override Task BeforeCombatStart()
        {
            HasBlockedAttackThisTurn = false;
            HasTakenDamageThisTurn = false;
            return Task.CompletedTask;
        }

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (props.HasFlag(ValueProp.Unpowered)) return;

            if (target == Owner.Creature)
            {
                if (result.BlockedDamage > 0)
                    HasBlockedAttackThisTurn = true;

                if (result.UnblockedDamage > 0)
                    HasTakenDamageThisTurn = true;
            }
            else if (target.PetOwner == Owner && dealer != null)
            {
                if (result.UnblockedDamage > 0)
                    HasTakenDamageThisTurn = true;
            }
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Enemy)
            {
                if (HasBlockedAttackThisTurn && !HasTakenDamageThisTurn && Owner.Creature.Block == 0)
                {
                    PerfectBlockCount++;
                    Flash();

                    if (PerfectBlockCount >= 5)
                    {
                        await RelicCmd.Replace(this, ModelDb.Relic<SpireExpert>().ToMutable());
                        return;
                    }
                }

                HasBlockedAttackThisTurn = false;
                HasTakenDamageThisTurn = false;
            }
        }
    }
}