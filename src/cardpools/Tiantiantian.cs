using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 甜甜甜 — 技能：花费20工资，征婚3次（升级4次）
	/// </summary>
	public sealed class Tiantiantian : XiaofujiuCardBase
	{
		public override bool HasMarriageEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		public override int SalaryCost => 20;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MarryCount", 3m)
		};

		public Tiantiantian()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await ZhengHunCmd.Marry(choiceContext, Owner, (int)DynamicVars["MarryCount"].BaseValue);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["MarryCount"].UpgradeValueBy(1m); // 3 → 4
		}
	}
}
