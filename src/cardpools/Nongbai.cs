using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;


namespace kyxiaofujiu.cardpools
{
	public sealed class Nongbai : XiaofujiuCardBase
	{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new EnergyVar(2),
			new PowerVar<StrengthPower>(4)  // 平衡性调整：力量 2→4
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<StrengthPower>());
				return tips;
			}
		}

		public Nongbai()
			: base(0, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int energyGain = IsUpgraded ? 3 : 2;
			await PlayerCmd.GainEnergy(energyGain, Owner);

			// 通过 Owner.Creature.CombatState 获取战斗状态
			var combatState = Owner.Creature.CombatState;
			// 获取所有敌人（对方的单位）
			var enemies = combatState.GetOpponentsOf(Owner.Creature);

			// 为所有敌人施加临时力量（本回合有效，回合结束自动消失；
			// 修复：原用 StrengthPower 永久加力量）
			foreach (var enemy in enemies)
			{
				await PowerCmd.Apply<NongbaiPower>(choiceContext, enemy, 4, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			// 升级时能量增加1
			if (DynamicVars.Energy != null)
				DynamicVars.Energy.UpgradeValueBy(1m);
		}
	}
}
