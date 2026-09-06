using System;
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
	/// 战斗之鹰 — 攻击（初始夫黑）：造成 10 点伤害（升级 14）
	/// </summary>
	public sealed class Zhandouzhiying : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.Black;

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new DamageVar(10m, ValueProp.Move)
		};

		public Zhandouzhiying()
			: base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)  // 改普通
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4m);
		}
	}
}
