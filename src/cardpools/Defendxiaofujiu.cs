using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Defendxiaofujiu : XiaofujiuCardBase
	{
		public override bool GainsBlock => true;

		protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Defend };

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new BlockVar(5m, ValueProp.Move)
		};

		// ✅ 不出现在卡牌奖励中
		public override bool CanBeGeneratedInCombat => false;
		public override bool CanBeGeneratedByModifiers => false;

		public Defendxiaofujiu()
			: base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3m);
		}
	}
}
