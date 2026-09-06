using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using MegaCrit.Sts2.Core.HoverTips;
using kyxiaofujiu.utils;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Nizenmehaizaizhe : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		// ✅ 显式声明为 DynamicVar[] 避免类型推断错误
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MarryCount", 4m),
			new BlockVar(7m, ValueProp.Move),        // 平衡性调整：格挡 5→7
			new IntVar("StrengthLoss", 1m)          // 平衡性调整：失力量 2→1
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

		// 征婚效果：基类统一添加征婚 + 夫态 hover
		public override bool HasMarriageEffect => true;

		public Nizenmehaizaizhe()
			: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int marryCount = (int)DynamicVars["MarryCount"].BaseValue;
			await ZhengHunCmd.Marry(choiceContext, Owner, marryCount);

			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			int strengthLoss = (int)DynamicVars["StrengthLoss"].BaseValue;
			await PowerCmd.Apply<StrengthPower>(
				choiceContext,
				Owner.Creature,
				-strengthLoss,
				Owner.Creature,
				this
			);
		}

		protected override void OnUpgrade()
		{
			var marryCount = (IntVar)DynamicVars["MarryCount"];
			marryCount.UpgradeValueBy(1m);

			DynamicVars.Block.UpgradeValueBy(2m);
		}
	}
}
