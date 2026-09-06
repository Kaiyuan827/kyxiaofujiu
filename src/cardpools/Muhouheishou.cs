using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 幕后黑手 — 根据手中夫黑数量获得额外格挡
	/// 夫白：获得 6 格挡 + 手牌中每张夫黑额外获得 3 格挡（升级 4）
	/// </summary>
	public sealed class Muhouheishou : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasBlackEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new BlockVar(6m, ValueProp.Move),
			new IntVar("BonusBlock", 3m)
		};

		public Muhouheishou()
			: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		// 计算手牌中的夫黑数量
		private int CountFufuBlackInHand()
		{
			if (Owner?.Piles == null) return 0;
			var hand = PileType.Hand.GetPile(Owner);
			if (hand == null) return 0;
			return hand.Cards.Count(c => c is XiaofujiuCardBase card && card.IsFufuBlack);
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int fufuBlackCount = CountFufuBlackInHand();
			int bonusPerCard = DynamicVars["BonusBlock"].IntValue;
			int totalBlock = (int)DynamicVars.Block.BaseValue + fufuBlackCount * bonusPerCard;

			await CreatureCmd.GainBlock(Owner.Creature, totalBlock, ValueProp.Move, cardPlay);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["BonusBlock"].UpgradeValueBy(1m);
		}
	}
}
