using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace kyxiaofujiu.powers
{
	public sealed class NeimaerdexinweiPower : PowerModel
	{
		private int _limit = 7;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		// Limit 动态变量：让 smartDescription 模板 {Limit} 能正确解析显示（值由施加卡升级状态同步）
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("Limit", 7m)
		};

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			// 从卡牌获取升级状态决定限制
			if (cardSource != null && cardSource.IsUpgraded)
			{
				_limit = 10;
			}
			else
			{
				_limit = 7;
			}
			// 同步到自身 DynamicVars（smartDescription 模板读取用）
			if (DynamicVars?["Limit"] is IntVar v) v.BaseValue = _limit;
			return Task.CompletedTask;
		}

		// 每回合开始时获得能量和抽牌
		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side != CombatSide.Player) return;
			if (Owner.IsDead) return;

			// 每层获得1点能量
			await PlayerCmd.GainEnergy(Amount, Owner.Player);

			// 每层多抽1张牌
			await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, Owner.Player);
		}

		// 限制出牌数量
		public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
		{
			// 只限制玩家手牌的手动打出（不限制自动打出）
			if (autoPlayType != AutoPlayType.None) return true;

			// 只限制自己的牌
			if (card.Owner != Owner.Player) return true;

			// 获取本回合已打出的牌数
			int playedThisTurn = CombatManager.Instance.History.CardPlaysStarted
				.Count(e => e.HappenedThisTurn(Owner.CombatState) && e.CardPlay.Card.Owner == Owner.Player);

			// 如果已打出牌数 >= 限制，则不能继续打出
			return playedThisTurn < _limit;
		}
	}
}
