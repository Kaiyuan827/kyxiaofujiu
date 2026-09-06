using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 卡九 — 每10回合获得1点能量并抽1张牌。
	/// 每次夫黑夫白转化可使计数器+1，但转化最多只能将计数器推到9，
	/// 只有回合开始的自动+1才能突破9触发效果。
	/// </summary>
	public sealed class KaJiuRelic : RelicModel
	{
		private int _counter;
		private bool _subscribed;

		public override RelicRarity Rarity => RelicRarity.Starter;

		public override bool ShowCounter => true;
		public override int DisplayAmount => _counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.ConvertTips;

		// 计数器（SL 持久化，防止 SL 后清空）
		[SavedProperty]
		public int Counter
		{
			get => _counter;
			set { AssertMutable(); _counter = value; InvokeDisplayAmountChanged(); }
		}

		public override Task BeforeCombatStart()
		{
			if (!_subscribed)
			{
				XiaofujiuCardBase.FufuStateChanged += OnFufuToggled;
				_subscribed = true;
			}
			return Task.CompletedTask;
		}

		public override Task AfterRemoved()
		{
			if (_subscribed)
			{
				XiaofujiuCardBase.FufuStateChanged -= OnFufuToggled;
				_subscribed = false;
			}
			return Task.CompletedTask;
		}

		// 战斗结束退订静态事件，缩小订阅窗口（转化只发生在战斗内；
		// 避免 SL/克隆等边界下旧实例长期驻留静态事件造成重复回调）
		public override Task AfterCombatEnd(CombatRoom room)
		{
			if (_subscribed)
			{
				XiaofujiuCardBase.FufuStateChanged -= OnFufuToggled;
				_subscribed = false;
			}
			return Task.CompletedTask;
		}

		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			// 只在玩家回合开始时触发（参考开心小花 HappyFlower）
			if (!participants.Contains(Owner.Creature)) return;

			// 回合开始自动 +1（这是唯一能突破9的方式）
			Counter++;

			// 达到10触发效果
			if (Counter >= 10)
			{
				Counter -= 10;
				await TriggerEffect();
			}
		}

		private void OnFufuToggled(CardModel card, FufuState oldState, FufuState newState)
		{
			// 只监听黑白之间的转换
			if (oldState == FufuState.None || newState == FufuState.None) return;
			if (oldState == newState) return;

			// 只监听自己玩家的卡牌
			if (card.Owner != Owner) return;
			if (Owner == null || Owner.Creature == null || Owner.Creature.IsDead) return;

			// 转化最多只能推到9，无法触发效果
			if (Counter >= 9) return;

			Counter++;

			Log.Info($"[KaJiuRelic] 转化触发, counter={Counter}");
		}

		private async Task TriggerEffect()
		{
			Flash();
			await PlayerCmd.GainEnergy(1, Owner);
			await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, Owner);
			Log.Info($"[KaJiuRelic] 触发效果: +1能量 +1抽牌, 剩余counter={_counter}");
		}
	}
}
