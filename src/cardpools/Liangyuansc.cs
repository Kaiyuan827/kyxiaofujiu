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
	/// 两元sc — 技能：花费20工资，将一张手牌转化为夫黄。消耗。升级费用-1
	/// </summary>
	public sealed class Liangyuansc : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;
		public override bool HasYellowEffect => true;
		public override bool HasConvertEffect => true;

		public override int SalaryCost => 20;

		public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

		public Liangyuansc()
			: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 选择一张手牌（夫黑/夫白）转化为夫黄
			var selected = await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "LIANGYUANSC.selectPrompt"), 0, 1),
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
			// 升级：移除消耗词条（费用仍是1）
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
