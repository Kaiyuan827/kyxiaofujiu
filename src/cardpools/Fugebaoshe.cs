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
	/// 夫哥爆射 — 对所有敌人造成10点伤害，攻击后转化自身（夫黑↔夫白翻转）
	/// </summary>
	public sealed class Fugebaoshe : XiaofujiuCardBase
	{
		public override bool HasBlackEffect => true;
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new DamageVar(10m, ValueProp.Move)
		};

		public Fugebaoshe()
			: base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 攻击后转化自身（翻转夫黑夫白状态）
			ToggleFufu();
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(5m);
		}
	}
}
