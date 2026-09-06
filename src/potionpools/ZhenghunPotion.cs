using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using kyxiaofujiu.commands;

namespace kyxiaofujiu.potionpools
{
	/// <summary>
	/// 征婚药水 — 使用后征婚 3 次
	/// 目标类型用 AnyPlayer（游戏本体战斗药水的标准类型，单机无需选择目标，直接对自己使用）
	/// </summary>
	public sealed class ZhenghunPotion : PotionModel
	{
		public override PotionRarity Rarity => PotionRarity.Common;

		public override PotionUsage Usage => PotionUsage.CombatOnly;

		public override TargetType TargetType => TargetType.AnyPlayer;

		protected override IEnumerable<DynamicVar> CanonicalVars => System.Array.Empty<DynamicVar>();

		protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
		{
			// 征婚 3 次
			await ZhengHunCmd.Marry(choiceContext, Owner, 3);
		}
	}
}
