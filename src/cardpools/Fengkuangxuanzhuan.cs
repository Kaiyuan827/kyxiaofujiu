using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;      // Debt（债务）
using MegaCrit.Sts2.Core.Models.Powers;     // WeakPower / VulnerablePower
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 疯狂旋转 — 攻击：造成 18 点伤害，给予 2 层虚弱和 2 层易伤。
	/// 你卡组中每有一张债务，额外攻击一次（每次攻击都是完整攻击：伤害 + 虚弱 + 易伤）。
	/// 升级：造成 25 点伤害，给予 3 层虚弱和 3 层易伤。
	/// </summary>
	public sealed class Fengkuangxuanzhuan : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;  // 初始夫白

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new DamageVar(18m, ValueProp.Move),
			new PowerVar<WeakPower>(2m),
			new PowerVar<VulnerablePower>(2m)
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
		{
			HoverTipFactory.FromPower<WeakPower>(),
			HoverTipFactory.FromPower<VulnerablePower>()
		};

		public Fengkuangxuanzhuan()
			: base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)  // 稀有
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var target = cardPlay.Target;
			if (target == null) return;

			// 攻击段数 = 1 + 卡组中债务数量
			int debtCount = Owner.Deck.Cards.Count(c => c is Debt);
			int hits = 1 + debtCount;

			// 每次攻击都是完整攻击：造成伤害 + 施加虚弱与易伤
			for (int i = 0; i < hits; i++)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
					.FromCard(this, cardPlay)
					.Targeting(target)
					.WithHitFx("vfx/vfx_attack_slash")
					.Execute(choiceContext);

				await PowerCmd.Apply<WeakPower>(choiceContext, target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
				await PowerCmd.Apply<VulnerablePower>(choiceContext, target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(7m);        // 18 → 25
			DynamicVars.Weak.UpgradeValueBy(1m);          // 2 → 3
			DynamicVars.Vulnerable.UpgradeValueBy(1m);    // 2 → 3
		}
	}
}
