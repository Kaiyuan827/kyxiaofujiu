using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 征个未来：X 费用技能卡，初始夫白，罕见。消耗。
	/// 征婚 2X 次并获得等量格挡；升级后征婚 3X 次并获得等量格挡。
	/// </summary>
	public sealed class Zhenggewelai : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasMarriageEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;  // 初始夫白

		public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };  // 消耗

		// X 费用卡：费用为 X
		protected override bool HasEnergyCostX => true;

		public Zhenggewelai()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)  // 罕见
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int x = ResolveEnergyXValue();

			// 征婚 2X 次（升级 3X），获得等量格挡（= 征婚次数）
			int marryCount = x * (IsUpgraded ? 3 : 2);
			int block = marryCount;

			await ZhengHunCmd.Marry(choiceContext, Owner, marryCount);
			await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
		}

		protected override void OnUpgrade()
		{
			// 升级倍率变化在 OnPlay 中用 IsUpgraded 判断（2X→3X，4X→5X）
		}
	}
}
