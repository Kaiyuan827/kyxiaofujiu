using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 无终恨意 — 攻击 2费：造成你已损失生命值的伤害。3回合内禁止征婚。消耗；升级：失去消耗
	/// </summary>
	public sealed class Wuzhonghenyi : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new CalculationBaseVar(0m),
			new ExtraDamageVar(1m),
			new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner.Creature.MaxHp - card.Owner.Creature.CurrentHp)
		};

		public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

		public Wuzhonghenyi()
			: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 3回合内禁止征婚（参考苦苦苦）
			await PowerCmd.Apply<NoMarryPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
			var noMarry = Owner.Creature.GetPower<NoMarryPower>();
			if (noMarry != null) noMarry.SkipNextDurationTick = false;
		}

		protected override void OnUpgrade()
		{
			// 升级：失去消耗
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
