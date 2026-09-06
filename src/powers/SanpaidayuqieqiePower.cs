using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;

namespace kyxiaofujiu.powers
{
    /// <summary>
    /// 三牌大于一切能力 — 回合结束时手牌夫黑=3，下回合多抽1
    /// </summary>
    public sealed class SanpaidayuqieqiePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        // 可叠加：Amount = 下回合额外抽牌数（每张+1，打两张多抽2）
        public override PowerStackType StackType => PowerStackType.Counter;

        private bool _bonusDrawNextTurn;

        // 回合结束时检查手牌夫黑数量
        public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side != CombatSide.Player) return Task.CompletedTask;
            if (!participants.Contains(Owner)) return Task.CompletedTask;
            if (Owner == null || Owner.IsDead) return Task.CompletedTask;

            var hand = PileType.Hand.GetPile(Owner.Player);
            if (hand == null) return Task.CompletedTask;

            int fufuBlack = hand.Cards.Count(c => c is XiaofujiuCardBase fc && fc.IsFufuBlack);
            _bonusDrawNextTurn = (fufuBlack == 3);

            return Task.CompletedTask;
        }

        // 回合开始时增加抽牌
        public override decimal ModifyHandDraw(Player player, decimal count)
        {
            if (player != Owner?.Player) return count;
            if (!_bonusDrawNextTurn) return count;
            _bonusDrawNextTurn = false;
            return count + Amount;
        }
    }
}
