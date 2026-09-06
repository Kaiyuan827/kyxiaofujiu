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
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 无视sc — 技能：获得 6 点格挡（升级 9），获得并转化弃牌堆的一张牌
	/// </summary>
	public sealed class Wushisc : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasConvertEffect => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
			{
				new BlockVar(6m, ValueProp.Move)  // 格挡6，升级9
		};

		public Wushisc()
			: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 获得 6/9 点格挡
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			// 从弃牌堆选一张牌，获得（回手）并转化
			var discard = PileType.Discard.GetPile(Owner);
			if (discard != null && discard.Cards.Count > 0)
			{
				var selected = await CardSelectCmd.FromCombatPile(
					choiceContext,
					discard,
					Owner,
					new CardSelectorPrefs(new LocString("cards", "WUSHISC.selectPrompt"), 0, 1),
					null);

				foreach (var card in selected)
				{
					await CardPileCmd.Add(card, PileType.Hand);  // 获得回手
					FufuCmd.Toggle(card);  // 转化（仅对夫卡生效）
				}
			}
		}

		protected override void OnUpgrade()
		{
			// 格挡 6 → 9
			DynamicVars.Block.UpgradeValueBy(3m);
		}
	}
}
