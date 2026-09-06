using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 五群来了 — 手牌数为10时可打出。将一张手牌转化为夫黄。消耗。
	/// </summary>
	public sealed class Wuqunlaile : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;
		public override bool HasYellowEffect => true;
		public override bool HasConvertEffect => true;

		public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

		public Wuqunlaile()
			: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		private bool IsHandTen()
		{
			// canonical（卡牌大全展示卡）无手牌，默认可显示
			if (IsCanonical) return true;
			var hand = Owner?.PlayerCombatState?.Hand;
			return hand != null && hand.Cards.Count == 10;
		}

		protected override bool IsPlayable
		{
			get
			{
				if (!base.IsPlayable) return false;
				return IsHandTen();
			}
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 将一张手牌（夫黑/夫白）转化为夫黄
			var selected = await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "WUQUNLAILE.selectPrompt"), 0, 1),
				c => c is XiaofujiuCardBase fc && (fc.IsFufuBlack || fc.IsFufuWhite),
				this);

			foreach (var card in selected)
			{
				if (card is XiaofujiuCardBase fc)
				{
					FufuCmd.ConvertToYellow(fc);
				}
			}
		}

		protected override void OnUpgrade()
		{
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
