using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;          // ← 新增，CardModel 所在命名空间
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Fubaimaomei : XiaofujiuCardBase
	{
		public override bool GainsBlock => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new BlockVar(16m, ValueProp.Move)
		};

		public Fubaimaomei()
			: base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(4m); // 16 → 20（平衡性调整）
		}
	}
}
