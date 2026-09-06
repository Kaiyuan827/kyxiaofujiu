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
	/// 碎心者 — 测试卡：花费工资的格挡技能卡
	/// </summary>
	public sealed class Suixinzhe : XiaofujiuCardBase
	{
		public override bool GainsBlock => true;

		public override FufuState CanonicalFufuState => FufuState.White;

		// 打出需要花费 20 工资（永久跨战斗保留资源，来自【幸运房东】遗物）
		public override int SalaryCost => 20;

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new BlockVar(10m, ValueProp.Move)
		};

		public Suixinzhe()
			: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(4m); // 10 → 14
		}
	}
}
