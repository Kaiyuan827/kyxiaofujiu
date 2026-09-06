using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;   // WeakPower / VulnerablePower / StrengthPower
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.cardpools
{
	public sealed class Oniupi : XiaofujiuCardBase
	{
		// ✅ 初始状态为夫白
		public override FufuState CanonicalFufuState => FufuState.White;

		// 效果随当前夫态变化（涉及夫黑/夫黄机制，hover 补充说明）
		public override bool HasBlackEffect => true;
		public override bool HasYellowEffect => true;

		// ✅ X 费用卡：费用为 X（投入 X 点能量，效果随 X 缩放，参考征个未来）
		protected override bool HasEnergyCostX => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new DamageVar(7m, ValueProp.Move)   // 基础 7，升级 10
		};

		// 虚弱/易伤/力量悬浮说明（数量随 X 动态变化，故用 hover 而非固定 PowerVar）
		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null)
				{
					tips.AddRange(baseTips);
				}
				tips.Add(HoverTipFactory.FromPower<WeakPower>());
				tips.Add(HoverTipFactory.FromPower<VulnerablePower>());
				tips.Add(HoverTipFactory.FromPower<StrengthPower>());
				return tips;
			}
		}

		public Oniupi()
			: base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)   // 稀有 X 费用
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var target = cardPlay.Target;
			if (target == null) return;

			int x = ResolveEnergyXValue();

			// 基础：造成 7/10 点伤害 X 次；夫黑状态：每段额外造成 X 点伤害
			int perHit = (int)DynamicVars.Damage.BaseValue + (IsFufuBlack ? x : 0);

			for (int i = 0; i < x; i++)
			{
				await DamageCmd.Attack(perHit)
					.FromCard(this, cardPlay)
					.Targeting(target)
					.WithHitFx("vfx/vfx_attack_slash")
					.Execute(choiceContext);
			}

			// 根据打出时的夫态附加效果
			if (IsFufuWhite)
			{
				// 夫白：给予 X 层虚弱和 X 层易伤
				await PowerCmd.Apply<WeakPower>(choiceContext, target, x, Owner.Creature, this);
				await PowerCmd.Apply<VulnerablePower>(choiceContext, target, x, Owner.Creature, this);
			}
			else if (IsFufuYellow)
			{
				// 夫黄：获得 X 点力量
				await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, x, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3m);  // 7 → 10
		}
	}
}
