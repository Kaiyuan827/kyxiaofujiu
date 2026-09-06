using System.Collections.Generic;
using System.Linq;
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
	/// 殊途同归 — 选择一张夫黑和一张夫白，第一张转化为夫黄，另一张消耗。消耗。
	/// </summary>
	public sealed class Shututonggui : XiaofujiuCardBase
	{
		public override bool HasBlackEffect => true;
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;
		public override bool HasYellowEffect => true;

		public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

		public Shututonggui()
			: base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var selected = (await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "SHUTUTONGGUI.selectPrompt"), 2),
				c => c is XiaofujiuCardBase fc && fc.IsFufu && !fc.IsFufuYellow && c != this,
				this
			)).ToList();

			if (selected.Count < 2) return;
			var card1 = selected[0] as XiaofujiuCardBase;
			var card2 = selected[1] as XiaofujiuCardBase;
			if (card1 == null || card2 == null) return;
			// 必须一黑一白才生效
			if (card1.IsFufuBlack == card2.IsFufuBlack) return;

			// 第一张转化为夫黄，另一张消耗
			FufuCmd.ConvertToYellow(card1);
			await CardPileCmd.Add(card2, PileType.Exhaust);
		}

		protected override void OnUpgrade()
		{
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
