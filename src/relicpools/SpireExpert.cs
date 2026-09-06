using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 尖塔高手 — 每场战斗开始时，获得1层缓冲（Buffer）
	/// </summary>
	public sealed class SpireExpert : RelicModel
	{
		public override RelicRarity Rarity => RelicRarity.Event;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
		{
			HoverTipFactory.FromPower<BufferPower>()
		};

		public override async Task BeforeCombatStart()
		{
			await PowerCmd.Apply<BufferPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, null, null);
		}
	}
}
