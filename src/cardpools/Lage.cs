using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 辣个 — 攻击：随机造成 4-12 点伤害（升级 4-18），并获得一半数量的格挡（重做）
	/// </summary>
	public sealed class Lage : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MinDamage", 4m),
			new IntVar("MaxDamage", 12m)  // 重做：上限12，升级18
		};

		public Lage()
			: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

			int min = (int)DynamicVars["MinDamage"].BaseValue;
			int max = (int)DynamicVars["MaxDamage"].BaseValue;
			// NextInt 的 max 是开区间，所以 max+1 才能包含上限
			int damage = Owner.RunState.Rng.CombatTargets.NextInt(min, max + 1);

			await DamageCmd.Attack(damage).FromCard(this, cardPlay).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 重做：获得一半数量的格挡（向下取整）
			int block = damage / 2;
			if (block > 0)
			{
				await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["MaxDamage"].UpgradeValueBy(6m);  // 12 → 18（重做）
		}
	}
}
