using System.Collections.Generic;
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
	/// 要嫁就嫁 — 双形态防御牌
	/// 
	/// 夫白：可直接打出，获得格挡，正常弃牌
	/// 夫黑：由外部效果转化而来，回合结束自动转为夫白并打出（获得格挡）
	/// </summary>
	public sealed class Yaojiajiujia : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasBlackEffect => true;
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new BlockVar(8m, ValueProp.Move)
		};

		public override bool HasTurnEndInHandEffect => IsFufuBlack;

		public Yaojiajiujia()
			: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		// ==================== 夫白时打出 ====================

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}

		// ==================== 夫黑回合结束自动打出 ====================

		protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
		{
			await base.OnTurnEndInHand(choiceContext);
			if (!IsFufuBlack) return;

			Unseal();
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
		}

		// ==================== 升级 ====================

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3m);
		}
	}
}
