using System.Collections.Generic;
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
	/// 带吧多带 — 技能：选择抽牌堆中的 2 张牌（升级 3 张），放到抽牌堆顶并转化。
	/// 转化 = 夫黑↔夫白切换（FufuCmd.Toggle，仅对夫卡生效）。
	/// </summary>
	public sealed class Daibaduodai : XiaofujiuCardBase
	{
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;  // 初始夫白

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("CardCount", 2m)  // 2张，升级3张
		};

		public Daibaduodai()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)  // 改0费，罕见
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var drawPile = PileType.Draw.GetPile(Owner);
			if (drawPile == null) return;

			int maxCount = (int)DynamicVars["CardCount"].BaseValue;
			var selected = await CardSelectCmd.FromCombatPile(
				choiceContext,
				drawPile,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "DAIBADUODAI.selectPrompt"), 0, maxCount),
				null
			);

			foreach (var card in selected)
			{
				// 放到抽牌堆顶
				await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
				// 转化（夫黑↔夫白，仅对夫卡生效）
				FufuCmd.Toggle(card);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["CardCount"].UpgradeValueBy(1m);  // 2 → 3
		}
	}
}
