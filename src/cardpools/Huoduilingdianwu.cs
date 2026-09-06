using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.powers;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Huoduilingdianwu : XiaofujiuCardBase
	{
		public Huoduilingdianwu()
			: base(4, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<HuoduilingdianwuPower>(
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
