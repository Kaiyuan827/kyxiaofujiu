using System;
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
	/// <summary>
	/// 双面夫哥 — 攻击 1费
	/// 造成 11/15 点伤害。
	/// 若本回合出的牌少于4张（第1~3张），则抽1张牌。
	/// 否则（第4张起），获得4点防御。
	/// </summary>
	public sealed class Shuangmianfuge : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		// 第 4 张起才给格挡：前 3 张仍是抽牌
		private const int PlayThreshold = 4;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new DamageVar(11m, ValueProp.Move),
			new BlockVar(4m, ValueProp.Move),
			new CardsVar(1)
		};

		public Shuangmianfuge()
			: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		/// <summary>
		/// 统计本回合玩家打出的卡牌数
		/// </summary>
		private int CardsPlayedThisTurn => CombatManager.Instance.History
			.CardPlaysStarted
			.Count(e => e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card.Owner == base.Owner);

		// 参考 Defect 的 FTL：前 3 张能触发抽牌时闪金光提示。
		// 注意：CardsPlayedThisTurn 在 OnPlay 时会把当前这张也算进去（CardPlayStarted 先于 OnPlay 写入），
		// 所以手牌展示时要用 < PlayThreshold - 1，正好对应第1~3张会抽牌、第4张起格挡。
		protected override bool ShouldGlowGoldInternal => base.CombatState != null && CardsPlayedThisTurn < PlayThreshold - 1;

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var target = cardPlay.Target;
			ArgumentNullException.ThrowIfNull(target, "cardPlay.Target");

			// 造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 判断条件：本回合出牌数 < 3 → 抽牌，否则 → 格挡
			if (CardsPlayedThisTurn < PlayThreshold)
			{
				await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
			}
			else
			{
				await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4m);
		}
	}
}
