using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 夫的女儿能力 — 回合结束时，手牌中有夫黑则获得 Amount 点格挡
	/// </summary>
	public sealed class FudenverPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		// 用 BeforeSideTurnEndEarly（早于灼伤等“回合结束手牌结算”），与金属化 Plating 一致：
		// 先发格挡 → 能挡住本回合结束阶段的灼伤伤害；此时手牌尚未丢弃，可正常检测手牌夫黑。
		public override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			// 只在玩家回合结束时检测
			if (side != CombatSide.Player) return;
			if (Owner == null || Owner.IsDead) return;
			if (Amount <= 0) return;

			// 检查手牌中是否有夫黑
			var hand = PileType.Hand.GetPile(Owner.Player);
			if (hand == null) return;

			bool hasFufuBlack = hand.Cards.OfType<XiaofujiuCardBase>().Any(c => c.IsFufuBlack);
			if (!hasFufuBlack) return;

			Flash();
			// 被动触发（非卡牌打出来源）的格挡不吃敏捷（Unpowered）
			await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
		}
	}
}
