using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
    /// <summary>
    /// 榜一什么东西 — 攻击牌，每次夫黑转夫白，这张牌本场战斗耗能-2
    /// </summary>
    public sealed class Bangyishenmedongxi : XiaofujiuCardBase
    {
        public override bool HasBlackEffect => true;
        public override bool HasConvertEffect => true;
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(66m, ValueProp.Move)
        };

        protected override int CanonicalEnergyCost => 10;

        public override FufuState CanonicalFufuState => FufuState.White;

        public Bangyishenmedongxi()
            : base(10, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(22m); // 66 → 88
        }

        private bool _subscribed;

        // 确保已订阅转化事件（去重；卡组里的牌不会触发 AfterCardEnteredCombat，
        // 需在抽到手牌时（AfterCardDrawn）也订阅）
        private void EnsureSubscribed()
        {
            if (_subscribed) return;
            XiaofujiuCardBase.FufuBlackToWhiteConverted += OnFufuBlackConverted;
            _subscribed = true;
            MegaCrit.Sts2.Core.Logging.Log.Info($"[Bangyishenmedongxi] 已订阅转化事件, 当前费用={EnergyCost.GetResolved()}, 夫态={FufuState}");
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            XiaofujiuCardBase.FufuBlackToWhiteConverted -= OnFufuBlackConverted;
            _subscribed = false;
        }

        // 战斗开始订阅：卡组里的牌在战斗开始时（Hook.BeforeCombatStart 遍历
        // player.Deck.Cards + 战斗牌堆全部卡）一定会收到此钩子，是最可靠时机。
        // 覆盖：卡组初始牌/首回合手牌/SL 后恢复战斗（重新走 StartCombatInternal）
        public override Task BeforeCombatStart()
        {
            EnsureSubscribed();
            return Task.CompletedTask;
        }

        // 入场时订阅（新加入牌堆的卡，如控制台添加/生成）
        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card != this) return Task.CompletedTask;
            EnsureSubscribed();
            return Task.CompletedTask;
        }

        // 抽到手牌时订阅（双保险，去重由 EnsureSubscribed 保证）
        public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card != this) return Task.CompletedTask;
            EnsureSubscribed();
            return Task.CompletedTask;
        }

        // 战斗结束时取消订阅
        public override Task AfterCombatEnd(CombatRoom room)
        {
            Unsubscribe();
            MegaCrit.Sts2.Core.Logging.Log.Info("[Bangyishenmedongxi] 已取消订阅");
            return Task.CompletedTask;
        }

        private void OnFufuBlackConverted(CardModel card)
        {
            // 只对本场战斗中的卡牌实例减费：卡组（Deck）里的牌 CombatState 为 null，
            // 也会订阅此静态事件，若同样减费，EndOfCombat 修饰符会跨战斗累积（卡组牌不会被清理），
            // 导致费用变成“全局永久减费”而非“本场战斗减费”。这里直接跳过即可。
            if (CombatState == null) return;

            // 每次夫黑转为夫白，本场战斗费用-2
            MegaCrit.Sts2.Core.Logging.Log.Info($"[Bangyishenmedongxi] 收到夫黑→夫白转化: {card}, 夫态={FufuState}, 减费前费用={EnergyCost.GetResolved()}");
            EnergyCost.AddThisCombat(-2);
            MegaCrit.Sts2.Core.Logging.Log.Info($"[Bangyishenmedongxi] 减费后费用={EnergyCost.GetResolved()}");
        }
    }
}
