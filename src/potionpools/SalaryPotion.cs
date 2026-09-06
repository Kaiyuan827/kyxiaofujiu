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
	/// 工资药水 — 获得 50 工资（走 SalaryCmd.Gain 统一入口，含爆米加成）
	/// 战斗内外均可使用（工资是跨战斗资源）
	/// </summary>
	public sealed class SalaryPotion : PotionModel
	{
		public override PotionRarity Rarity => PotionRarity.Uncommon;

		public override PotionUsage Usage => PotionUsage.AnyTime;

		public override TargetType TargetType => TargetType.AnyPlayer;

		protected override IEnumerable<DynamicVar> CanonicalVars => System.Array.Empty<DynamicVar>();

		protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
		{
			SalaryCmd.Gain(Owner, 50);
			await Task.CompletedTask;
		}
	}
}
