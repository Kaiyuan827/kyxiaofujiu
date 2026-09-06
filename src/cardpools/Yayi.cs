using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Yayi : XiaofujiuCardBase
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new DamageVar(40m, ValueProp.Move)  // 40伤害，升级55
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<WeakPower>());
				tips.Add(HoverTipFactory.FromPower<VulnerablePower>());
				return tips;
			}
		}

		public Yayi()
			: base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)  // 稀有度改罕见
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var target = cardPlay.Target;
			if (target == null) return;

			var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(target)
				.WithHitFx("vfx/vfx_attack_slash");
			await attack.Execute(choiceContext);

			// 若没有击杀敌人，则获得2层虚弱和易伤（本回合生效，本回合结束扣除）
			bool killed = attack.Results.SelectMany(r => r).Any(r => r.WasTargetKilled);
			if (!killed)
			{
				await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
				var weak = Owner.Creature.GetPower<WeakPower>();
				if (weak != null) weak.SkipNextDurationTick = false;

				await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
				var vuln = Owner.Creature.GetPower<VulnerablePower>();
				if (vuln != null) vuln.SkipNextDurationTick = false;
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(15m);  // 40 → 55
		}
	}
}
