using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;  // ✅ Creature
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Helpers;              // ✅ TaskHelper
using MegaCrit.Sts2.Core.Logging;              // ✅ Log
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	public sealed class YiciyigePower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.ConvertTips;

		/// <summary>
		/// 回合结束阶段（CombatManager.DoTurnEnd 中）抽的牌要保留（GiveSingleTurnRetain），
		/// 避免"要嫁就嫁"回合结束转化触发抽牌后立即被弃掉。
		/// 由 patch CombatManager.DoTurnEnd 在回合结束期间置 true、结束后清 false。
		/// </summary>
		/// <summary>
		/// 回合结束阶段标志（patch CombatManager.DoTurnEnd 置 true / 弃牌完成后清 false）。
		/// 多人说明：该标志表示"当前正处于玩家侧回合结束流程"，co-op 中所有玩家共享同一次
		/// 回合结束阶段，因此静态共享是符合语义的（回合结束期间各玩家的要嫁就嫁/一次一个抽牌都该保留）。
		/// </summary>
		public static bool RetainDrawnAtCombatEnd;

		/// <summary>
		/// 由 XiaofujiuCardBase.NotifyOwnerPowers 调用（夫黑→夫白转化时）。
		/// 走"生物上的 Power 钩子"而非静态事件订阅——战斗结束游戏用
		/// RemoveAllPowersInternalExcept 清理 Power（跳过 AfterRemoved），
		/// 静态订阅会跨战斗残留/叠加导致抽牌翻倍；改为查生物上的 Power 则
		/// 战斗结束 Power 移除后自动失效，不会残留。
		/// </summary>
		public void OnFufuBlackToWhiteConverted(CardModel card)
		{
			if (Owner == null || Owner.IsDead) return;
			if (Owner.Player == null) return;

			Log.Info($"=== YiciyigePower: 夫黑→夫白转换触发（层数：{Amount}，抽 {Amount} 张牌）===");
			Flash();

			// 事件是同步回调，抽牌用 RunSafely 跑（不用 async void，避免未捕获异常崩/卡死）
			TaskHelper.RunSafely(DrawCards());
		}

		private async Task DrawCards()
		{
			var drawn = (await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, Owner.Player)).ToList();

			// 回合结束阶段抽的牌给保留（本回合结束不弃，下回合继续在手牌）
			if (RetainDrawnAtCombatEnd)
			{
				foreach (var c in drawn)
				{
					c.GiveSingleTurnRetain();
				}
			}
		}

		public override bool ShouldReceiveCombatHooks => true;
	}
}
