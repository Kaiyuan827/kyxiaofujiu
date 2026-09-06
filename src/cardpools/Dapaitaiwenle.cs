using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Dapaitaiwenle : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		private static decimal GetBlockMultiplier(CardModel card, Creature? target)
		{
			// 计算本回合已出牌数
			int playedCount = CombatManager.Instance.History.CardPlaysFinished
				.Count(e => e.HappenedThisTurn(card.CombatState)
							&& e.CardPlay.Card.Owner == card.Owner);

			// 只返回出牌数（×2 由 CalculationExtraVar(2) 提供：最终格挡 = base0 + 2 × playedCount）
			return playedCount;
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new CalculationBaseVar(0m),                    // 基础值
			new CalculationExtraVar(2m),                   // 倍数
			new CalculatedVar("Block").WithMultiplier(GetBlockMultiplier)  // 最终计算值
		};

		public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
		{
			CardKeyword.Exhaust
		};

		public override bool CanBeGeneratedInCombat => true;
		public override bool CanBeGeneratedByModifiers => true;

		public Dapaitaiwenle()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)  // 稀有度改罕见
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// ✅ 使用 CalculatedVar 计算格挡值
			var calculatedBlock = DynamicVars["Block"] as CalculatedVar;
			decimal blockAmount = calculatedBlock?.Calculate(cardPlay.Target) ?? 0m;

			await CreatureCmd.GainBlock(Owner.Creature, blockAmount, ValueProp.Move, cardPlay);
		}

		protected override void OnUpgrade()
		{
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
