using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 夫的女儿 — 能力牌，回合结束时手牌有夫黑则获得格挡
	/// </summary>
	public sealed class Fudenver : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasBlackEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			// Unpowered：该格挡由能力在回合结束被动发放（不吃敏捷）；
			// 卡面显示与 FudenverPower 实际发放的 Unpowered 格挡保持一致
			new BlockVar(4m, ValueProp.Unpowered)
		};

		public Fudenver()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<FudenverPower>(choiceContext, Owner.Creature, (int)DynamicVars.Block.BaseValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(2m);
		}
	}
}
