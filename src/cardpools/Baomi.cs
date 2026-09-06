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
	public sealed class Baomi : XiaofujiuCardBase
	{
		public override bool ShowSalaryHover => true;

		// 每次打出获得的额外工资（数值以 SalaryGain 变量统一，供卡面与 power 共用）
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("SalaryGain", 15m)
		};

		public Baomi()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<BaomiPower>(choiceContext, Owner.Creature, DynamicVars["SalaryGain"].BaseValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			// 费用从1变为0
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
