using System.Collections.Generic;
using System.Linq;  // ✅ 添加此项
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.cardpools
{
	public sealed class Heizhuanbaizhuanhei : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasBlackEffect => true;
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new BlockVar(3m, ValueProp.Move),   // 重做：格挡 3
			new IntVar("CardCount", 1m)          // 重做：至多1张，升级2张
		};

		public Heizhuanbaizhuanhei()
			: base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)  // 重做：0费
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			// 选择至多 maxCount 张手牌转化（可跳过/少选）
			int maxCount = (int)DynamicVars["CardCount"].BaseValue;
			var selected = await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "HEIZHUANBAIZHUANHEI.selectPrompt"), 0, maxCount),
				c => c is XiaofujiuCardBase f && f.IsFufu && !f.IsFufuYellow,
				this
			);

			foreach (var card in selected)
			{
				if (card is XiaofujiuCardBase fufuCard)
				{
					FufuCmd.Toggle(fufuCard);
				}
			}
		}

		protected override void OnUpgrade()
		{
			// 重做：升级至多2张（费用保持0）
			DynamicVars["CardCount"].UpgradeValueBy(1m);  // 1 → 2
		}
	}
}
