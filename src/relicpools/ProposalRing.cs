using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 征婚戒指 — 每场战斗开始时，获得10格挡，征婚3次
	/// </summary>
	public sealed class ProposalRing : RelicModel
	{
		public override RelicRarity Rarity => RelicRarity.Event;

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				foreach (var tip in HoverTipHelper.MarriageTips) yield return tip;
				yield return HoverTipFactory.Static(StaticHoverTip.Block);
			}
		}

		public override async Task BeforeCombatStart()
		{
			await CreatureCmd.GainBlock(Owner.Creature, 10, ValueProp.Move | ValueProp.Unpowered, null);
			await ZhengHunCmd.Marry(new ThrowingPlayerChoiceContext(), Owner, 3);
		}
	}
}
