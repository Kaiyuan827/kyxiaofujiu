using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	public sealed class Yiciyige : XiaofujiuCardBase
	{
		public override bool HasBlackEffect => true;
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		public Yiciyige()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<YiciyigePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			// 费用从1变为0
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
