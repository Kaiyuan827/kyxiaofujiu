using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.potionpools
{
	/// <summary>
	/// 夫黄药水 — 选择手牌中的一张夫黑/夫白牌，本场战斗将其转化为夫黄
	/// 仅战斗中可用（需要手牌）
	/// </summary>
	public sealed class FufuYellowPotion : PotionModel
	{
		public override PotionRarity Rarity => PotionRarity.Rare;

		public override PotionUsage Usage => PotionUsage.CombatOnly;

		public override TargetType TargetType => TargetType.AnyPlayer;

		protected override IEnumerable<DynamicVar> CanonicalVars => System.Array.Empty<DynamicVar>();

		protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
		{
			// 从手牌中选一张夫黑/夫白牌
			var prefs = new CardSelectorPrefs(
				new LocString("potions", "FUFU_YELLOW_POTION.selectionScreenPrompt"), 1, 1);
			var selected = await CardSelectCmd.FromHand(choiceContext, Owner, prefs,
				c => c is XiaofujiuCardBase f && (f.IsFufuBlack || f.IsFufuWhite), this);

			var card = selected.FirstOrDefault();
			if (card != null)
			{
				FufuCmd.ConvertToYellow(card);
			}
		}
	}
}
