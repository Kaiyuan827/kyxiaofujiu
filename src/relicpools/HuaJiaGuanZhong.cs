using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 花嫁观众 — 每场战斗你第一次将夫黑转化为夫白时，直接将其打出。
	/// 订阅静态转化事件（战斗开始订阅/战斗结束退订，防跨战斗残留）。
	/// </summary>
	public sealed class HuaJiaGuanZhong : RelicModel
	{
		private bool _usedThisCombat;
		private bool _subscribed;

		public override RelicRarity Rarity => RelicRarity.Shop;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.ConvertTips;

		protected override IEnumerable<DynamicVar> CanonicalVars => System.Array.Empty<DynamicVar>();

		public override Task BeforeCombatStart()
		{
			_usedThisCombat = false;
			if (!_subscribed)
			{
				XiaofujiuCardBase.FufuBlackToWhiteConverted += OnFufuBlackToWhite;
				_subscribed = true;
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatEnd(CombatRoom room)
		{
			if (_subscribed)
			{
				XiaofujiuCardBase.FufuBlackToWhiteConverted -= OnFufuBlackToWhite;
				_subscribed = false;
			}
			return Task.CompletedTask;
		}

		private void OnFufuBlackToWhite(CardModel card)
		{
			if (_usedThisCombat) return;
			if (card.IsCanonical || card.Owner != Owner) return;
			// 卡必须还在手牌：要嫁就嫁回合结束是先移入 Play 堆再转化，
			// 那种情况不重复打出（否则会双重打出）
			if (!PileType.Hand.GetPile(Owner).Cards.Contains(card)) return;

			_usedThisCombat = true;
			Flash();
			TaskHelper.RunSafely(AutoPlayCard(card));
		}

		private async Task AutoPlayCard(CardModel card)
		{
			await CardCmd.AutoPlay(new ThrowingPlayerChoiceContext(), card, null);
		}
	}
}
