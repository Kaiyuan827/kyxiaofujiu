using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;

using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	public sealed class Xiaofujiushiliansheng : XiaofujiuCardBase
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new IntVar("Threshold", 5m)  // 升级后变为7
		};

		public Xiaofujiushiliansheng()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

			// 根据升级状态决定本次施加的层数（阈值贡献值）
			int amount = IsUpgraded ? 7 : 5;

			// 每次施加都会叠加到已有的 Amount 上
			await PowerCmd.Apply<XiaofujiushilianshengPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			var threshold = (IntVar)DynamicVars["Threshold"];
			threshold.UpgradeValueBy(2m);  // 5 → 7
		}
	}
}
