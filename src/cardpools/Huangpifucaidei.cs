using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 黄皮夫才对 — 能力：夫黄获得额外重放1
	/// </summary>
	public sealed class Huangpifucaidei : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;
		public override bool HasYellowEffect => true;

		public Huangpifucaidei()
			: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<HuangpifucaideiPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
			// 立即刷新现有夫黄卡的重放次数
			XiaofujiuCardBase.RefreshReplayCounts(Owner);
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1); // 费用 1 → 0
		}
	}
}
