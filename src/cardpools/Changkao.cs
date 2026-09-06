using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	public sealed class Changkao : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new BlockVar(1m, ValueProp.Move),
			new PowerVar<DexterityPower>(1m)
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<DexterityPower>());
				return tips;
			}
		}

		public override bool CanBeGeneratedInCombat => false;
		public override bool CanBeGeneratedByModifiers => false;

		public Changkao()
			: base(0, CardType.Skill, CardRarity.Event, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			await PowerCmd.Apply<ChangkaoPower>(
				choiceContext,
				Owner.Creature,
				DynamicVars.Dexterity.BaseValue,
				Owner.Creature,
				this
			);
			await CardPileCmd.Draw(choiceContext, 1, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Dexterity.UpgradeValueBy(2m);
		}
	}
}
