using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 放干夫血 — 技能：花费30工资，获得2能量（升级3）
	/// </summary>
	public sealed class Fangganfuxue : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;

		public override int SalaryCost => 30;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new EnergyVar(2)
		};

		public Fangganfuxue()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PlayerCmd.GainEnergy((int)DynamicVars.Energy.BaseValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Energy.UpgradeValueBy(1m); // 2 → 3
		}
	}
}
