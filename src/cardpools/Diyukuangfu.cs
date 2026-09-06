using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 地狱狂夫 — 能力：每当你抽到夫黑时，将其打出。升级：费用 -1。
	/// 参考游戏 Hellraiser（2 费 Power，抽到打击自动打出，升级费用-1）。
	/// </summary>
	public sealed class Diyukuangfu : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;  // 初始夫白
		public override bool HasBlackEffect => true;

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.AddRange(HoverTipHelper.FuheiPlayTips);
				return tips;
			}
		}

		public Diyukuangfu()
			: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)  // 稀有
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<DiyukuangfuPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);  // 费用 2 → 1
		}
	}
}
