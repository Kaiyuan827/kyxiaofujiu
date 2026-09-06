using System.Collections.Generic;
using System.Linq;          // ✅ 添加
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
	public sealed class Qinggetanchang : XiaofujiuCardBase
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]   // ✅ 显式类型
		{
			new DamageVar(5m, ValueProp.Move),
			new PowerVar<VulnerablePower>(1m)
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<VulnerablePower>());
				return tips;
			}
		}

		public Qinggetanchang()
			: base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.TargetingAllOpponents(CombatState)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			var enemies = CombatState.Enemies.Where(c => c.IsAlive).ToList();  // ✅ 使用 Enemies
			foreach (var enemy in enemies)
			{
				await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2m);
			DynamicVars.Vulnerable.UpgradeValueBy(1m);
		}
	}
}
