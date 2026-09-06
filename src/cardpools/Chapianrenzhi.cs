using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.powers;
using MegaCrit.Sts2.Core.HoverTips;
using kyxiaofujiu.utils;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Chapianrenzhi : XiaofujiuCardBase
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MarryCount", 3m)
		};

		// 征婚效果：基类统一添加征婚 + 夫态 hover
		public override bool HasMarriageEffect => true;

		public Chapianrenzhi()
			: base(3, CardType.Power, CardRarity.Ancient, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<ChapianrenzhiPower>(choiceContext, Owner.Creature, DynamicVars["MarryCount"].BaseValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			var marryCount = (IntVar)DynamicVars["MarryCount"];
			marryCount.UpgradeValueBy(1m); // 3 → 4
		}
	}
}
