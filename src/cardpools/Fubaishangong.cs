using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.powers;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Fubaishangong : XiaofujiuCardBase
	{
		public override bool ShowSalaryHover => true;
		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.Static(StaticHoverTip.Fatal));
				return tips;
			}
		}
		// ✅ 添加动态变量用于升级显示
		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new IntVar("SalaryGain", 20m)
		};

		public Fubaishangong()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
			// 施加 Amount = 工资数（可叠加：每张夫白上供 +SalaryGain，升级+30）
			int salaryGain = (int)DynamicVars["SalaryGain"].BaseValue;
			await PowerCmd.Apply<FubaishangongPower>(choiceContext, Owner.Creature, salaryGain, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			// ✅ 工资20→30
			DynamicVars["SalaryGain"].UpgradeValueBy(10m);
		}
	}
}
