using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	public sealed class Tufaeji : XiaofujiuCardBase
	{
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new EnergyVar(2),
			new IntVar("Count", 2m)
		};

		public Tufaeji()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)  // 改罕见
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PlayerCmd.GainEnergy((int)DynamicVars.Energy.BaseValue, Owner);

			var hand = PileType.Hand.GetPile(Owner);
			var fufuCards = hand.Cards
				.Where(c => c is XiaofujiuCardBase fc && fc.IsFufu)
				.ToList();

			int toToggle = Math.Min(DynamicVars["Count"].IntValue, fufuCards.Count);
			// 多人安全：用玩家的确定性 RNG 挑选要转化的夫黑卡
			var rng = Owner?.RunState?.Rng?.CombatCardSelection;
			for (int i = 0; i < toToggle; i++)
			{
				int idx = rng != null ? rng.NextInt(0, fufuCards.Count) : 0;
				var card = fufuCards[idx];
				FufuCmd.Toggle(card);
				fufuCards.RemoveAt(idx);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Energy.UpgradeValueBy(1m);
		}
	}
}
