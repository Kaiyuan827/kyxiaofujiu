using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 开夫眼 — 技能：从抽牌堆选择 1 张夫白卡加入手牌（升级2张），消耗
	/// </summary>
	public sealed class Kaifuyan : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("CardCount", 1m)
		};

		public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

		public Kaifuyan()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int count = (int)DynamicVars["CardCount"].BaseValue;

			// 从抽牌堆选择 count 张夫白卡加入手牌
			var selected = await CardSelectCmd.FromCombatPile(
				choiceContext,
				PileType.Draw.GetPile(Owner),
				Owner,
				new CardSelectorPrefs(new LocString("cards", "KAIFUYAN.selectPrompt"), count),
				c => c is XiaofujiuCardBase fc && fc.IsFufuWhite);

			foreach (var card in selected)
			{
				await CardPileCmd.Add(card, PileType.Hand);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["CardCount"].UpgradeValueBy(1m);
		}
	}
}
