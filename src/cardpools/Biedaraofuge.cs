using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;          // ✅ 添加
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.cardpools;                // ✅ 引用 Jiaban
using kyxiaofujiu.powers;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Biedaraofuge : XiaofujiuCardBase
	{
		// ✅ 添加：鼠标悬停时显示“加班”卡面
		protected override IEnumerable<IHoverTip> ExtraHoverTips 
			=> new IHoverTip[] { HoverTipFactory.FromCard<Jiaban>() };

		public Biedaraofuge()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		public override IEnumerable<CardKeyword> CanonicalKeywords
		{
			get
			{
				if (IsUpgraded)
				{
					return new[] { CardKeyword.Innate };
				}
				return new CardKeyword[] { };
			}
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			await PowerCmd.Apply<BiedaraofugePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			AddKeyword(CardKeyword.Innate);
		}
	}
}
