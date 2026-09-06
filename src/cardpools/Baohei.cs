using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 豹黑 — 技能：抽 3 张牌（升级 4），选择手牌中的一张转化。普通
	/// </summary>
	public sealed class Baohei : XiaofujiuCardBase
	{
		public override bool HasConvertEffect => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("DrawCount", 3m)  // 抽3张，升级4张
		};

		public Baohei()
			: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)  // 普通
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 抽 3 张牌（升级 4）
			await CardPileCmd.Draw(choiceContext, (int)DynamicVars["DrawCount"].BaseValue, Owner);

			// 选择手牌中的一张转化（夫黑↔夫白，仅对夫卡生效）
			var selected = await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "BAOHEI.selectPrompt"), 0, 1),
				c => c is XiaofujiuCardBase fc && (fc.IsFufuBlack || fc.IsFufuWhite),
				this);

			foreach (var card in selected)
			{
				FufuCmd.Toggle(card);
			}
		}

		protected override void OnUpgrade()
		{
			// 抽牌数 3 → 4
			DynamicVars["DrawCount"].UpgradeValueBy(1m);
		}
	}
}
