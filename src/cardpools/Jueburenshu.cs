using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.powers;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Jueburenshu : XiaofujiuCardBase
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
		{
			CardKeyword.Retain,
			CardKeyword.Exhaust
		};

		public Jueburenshu()
			: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<JueburenshuPower>(
				choiceContext,
				Owner.Creature,
				1m,
				Owner.Creature,
				this
			);
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
