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
	/// 不为你而死 — 能力牌 0费：先征婚4次，再获得"征婚不消耗生命、蛇花小姐不再承受伤害"的能力。升级征婚6次
	/// </summary>
	public sealed class Buweinierersi : XiaofujiuCardBase
	{
		public override bool HasMarriageEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MarryCount", 4m)
		};

		public Buweinierersi()
			: base(0, CardType.Power, CardRarity.Rare, TargetType.Self)  // 改0费
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

			// 注意：先征婚 3 次（此时蛇花小姐仍会正常承受伤害），再添加能力
			await ZhengHunCmd.Marry(choiceContext, Owner, (int)DynamicVars["MarryCount"].BaseValue);

			// 征婚不再消耗生命，蛇花小姐也不再承受伤害
			await PowerCmd.Apply<BuweinierersiPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			// 升级：征婚 4 → 6 次（不再减费）
			DynamicVars["MarryCount"].UpgradeValueBy(2m);
		}
	}
}
