using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.powers;
using MegaCrit.Sts2.Core.HoverTips; 
using kyxiaofujiu.utils; 
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Zhenghunzhe : XiaofujiuCardBase
	{
		public Zhenghunzhe()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}
		
		// 征婚效果：基类统一添加征婚 + 夫态 hover
		public override bool HasMarriageEffect => true;

		// ✅ 模板中不定义固有，仅通过升级动态添加
		public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { };

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<ZhenghunzhePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			// ✅ 升级时动态添加固有关键字
			AddKeyword(CardKeyword.Innate);
		}
	}
}
