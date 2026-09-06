using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.powers;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Yayixingtai : XiaofujiuCardBase
	{
		public override bool HasMarriageEffect => true;
		// ✅ 升级前有虚无，升级后通过 RemoveKeyword 动态移除
		public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
		{
			CardKeyword.Ethereal
		};

		public Yayixingtai()
			: base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<YayixingtaiPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			// ✅ 升级后动态移除虚无关键词
			RemoveKeyword(CardKeyword.Ethereal);
		}
	}
}
