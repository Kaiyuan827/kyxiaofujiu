using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 卡遗物 — 每回合+1，战斗结束时若为3累计进度，满5次变为卡好了
	/// </summary>
	public sealed class KaRelic : RelicModel
	{
		private int _turnCounter;
		private int _progressCounter;

		public override RelicRarity Rarity => RelicRarity.Event;

		public override bool ShowCounter => IsMutable;
		public override int DisplayAmount => _turnCounter;

		// 回合计数（跨战斗保留进度是设计意图；SL 持久化防进度丢失）
		[SavedProperty]
		public int TurnCounter
		{
			get => _turnCounter;
			set
			{
				AssertMutable();
				_turnCounter = value;
				InvokeDisplayAmountChanged();
			}
		}

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				if (!IsMutable) yield break; // 图鉴（模板）不显示进度悬浮窗
				var desc = new LocString("relics", "KA_RELIC.progressDesc");
				desc.Add("Current", _progressCounter);
				desc.Add("Total", 5);
				yield return new HoverTip(desc); // 单参构造 → Title=null，无标题行，不留空行
			}
		}

		[SavedProperty]
		public int ProgressCounter
		{
			get => _progressCounter;
			set
			{
				AssertMutable();
				_progressCounter = value;
			}
		}

		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (!participants.Contains(Owner.Creature)) return;
			if (Owner.Creature.IsDead) return;

			TurnCounter = (TurnCounter + 1) % 4;
		}

		public override async Task AfterCombatVictory(CombatRoom room)
		{
			if (_turnCounter == 3)
			{
				ProgressCounter++;
				Flash();

				if (ProgressCounter >= 5)
				{
					await RelicCmd.Replace(this, ModelDb.Relic<KaHaoLe>().ToMutable());
				}
			}
		}
	}
}
