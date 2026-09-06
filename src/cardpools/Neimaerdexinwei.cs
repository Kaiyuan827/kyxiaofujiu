using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.powers;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Neimaerdexinwei : XiaofujiuCardBase
	{
		public Neimaerdexinwei()
			: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<NeimaerdexinweiPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			// 费用从2变为1
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
