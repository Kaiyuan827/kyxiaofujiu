using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 勒紧我 — 技能：征婚，蛇花小姐本回合协助攻击（打出攻击牌后额外造成一段伤害）
	/// </summary>
	public sealed class Lejinwo : XiaofujiuCardBase
	{
		public override bool HasMarriageEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MarryCount", 2m)
		};

		public Lejinwo()
			: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 征婚 2 次（升级 3）
			await ZhengHunCmd.Marry(choiceContext, Owner, (int)DynamicVars["MarryCount"].BaseValue);

			// 本回合蛇花小姐协助攻击
			await PowerCmd.Apply<LejinwoPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["MarryCount"].UpgradeValueBy(1m);
		}
	}
}
