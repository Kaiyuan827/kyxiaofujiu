using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 追踪之蛇 — 能力牌：征婚，蛇花小姐对所有敌人造成伤害
	/// </summary>
	public sealed class Zhuizongzhishe : XiaofujiuCardBase
	{
		public override bool HasMarriageEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MarryCount", 2m)
		};

		public Zhuizongzhishe()
			: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

			// 征婚 2 次（升级 4）
			await ZhengHunCmd.Marry(choiceContext, Owner, (int)DynamicVars["MarryCount"].BaseValue);

			// 蛇花小姐可以对所有敌人造成伤害
			await PowerCmd.Apply<ZhuizongzhishePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["MarryCount"].UpgradeValueBy(2m);
		}
	}
}
