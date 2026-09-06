using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using kyxiaofujiu.commands;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 尖塔压抑 — 每3回合征婚2。
	/// 计数跨战斗保留、SL 不清空（[SavedProperty]）。
	/// </summary>
	public sealed class JianTaYaYi : RelicModel
	{
		private const int _interval = 3;
		private int _turnCounter;

		public override RelicRarity Rarity => RelicRarity.Uncommon;

		public override bool ShowCounter => true;
		public override int DisplayAmount => _turnCounter;  // 计数递增显示（0→1→2→3），与游戏内计数遗物一致

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;

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

		protected override IEnumerable<DynamicVar> CanonicalVars => System.Array.Empty<DynamicVar>();

		// 玩家回合开始时计数，满3回合征婚2
		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner) return;
			if (Owner.Creature == null || Owner.Creature.IsDead) return;

			TurnCounter++;
			if (TurnCounter >= _interval)
			{
				TurnCounter = 0;
				Flash();
				await ZhengHunCmd.Marry(choiceContext, Owner, 2);
			}
		}
	}
}
