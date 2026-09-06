using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Chidafen : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasMarriageEffect => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("CardCount", 3m),
			new BlockVar(3m, ValueProp.Move)
		};

		public Chidafen()
			: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int maxCount = (int)DynamicVars["CardCount"].BaseValue;

			// 选择最多 maxCount 张手牌消耗（最少0张）
			var selected = await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "CHIDAFEN.selectPrompt"), 0, maxCount),
				null,
				this
			);

			foreach (var card in selected)
			{
				// 消耗选中的牌
				await CardCmd.Exhaust(choiceContext, card);

				// 每张牌提供3点格挡
				await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

				// 如果是状态牌，额外征婚1次
				if (card.Type == CardType.Status)
				{
					await ZhengHunCmd.Marry(choiceContext, Owner, 1);
				}
			}
		}

		protected override void OnUpgrade()
		{
			// 选择数量从3变为5
			var count = (IntVar)DynamicVars["CardCount"];
			count.UpgradeValueBy(2m);
		}
	}
}
