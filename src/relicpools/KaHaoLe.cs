using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 卡好了 — 每场战斗开始：抽1牌 +1能量，下一张攻击牌伤害翻倍
	/// </summary>
	public sealed class KaHaoLe : RelicModel
	{
		private bool _nextAttackDoubled;

		public override RelicRarity Rarity => RelicRarity.Event;

		public override bool ShowCounter => false;

		// 下一张攻击翻倍标记（SL 持久化，防 SL 回战斗中丢失）
		[SavedProperty]
		public bool NextAttackDoubled
		{
			get => _nextAttackDoubled;
			set { AssertMutable(); _nextAttackDoubled = value; }
		}

		public override Task BeforeCombatStart()
		{
			NextAttackDoubled = false;
			return Task.CompletedTask;
		}

		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (!participants.Contains(Owner.Creature)) return;
			// 仅第一回合触发（参考灯笼 Lantern）
			if (Owner.PlayerCombatState.TurnNumber > 1) return;

			Flash();
			await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, Owner);
			await PlayerCmd.GainEnergy(1, Owner);
			NextAttackDoubled = true;
		}

		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
		{
			if (!NextAttackDoubled) return 1m;
			if (!props.IsPoweredAttack()) return 1m;
			if (dealer != Owner.Creature && dealer != Owner.Osty) return 1m;
			if (cardSource == null || cardSource.Type != CardType.Attack) return 1m;
			return 2m;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (NextAttackDoubled && cardPlay.Card.Owner == Owner && cardPlay.Card.Type == CardType.Attack)
			{
				NextAttackDoubled = false;
			}
		}
	}
}
