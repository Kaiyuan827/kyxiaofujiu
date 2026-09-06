using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;


namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 装唐 — 技能 3费，稀有，消耗：结束你的回合。玩家回合开始时眩晕所有敌人 → 下一轮敌人全部被晕。升级费用-1
	/// </summary>
	public sealed class Zhuangtang : XiaofujiuCardBase
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };  // 消耗

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.Static(StaticHoverTip.Stun));
				return tips;
			}
		}

		public Zhuangtang()
			: base(3, CardType.Skill, CardRarity.Rare, TargetType.None)  // 3费，稀有，无目标（眩晕所有敌人）
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 应用延时眩晕（挂在玩家身上）：本轮敌人正常行动，玩家回合开始时眩晕所有敌人 → 下一轮全部被晕
			await PowerCmd.Apply<ZhuangtangStunPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

			// 结束你的回合
			PlayerCmd.EndTurn(Owner, canBackOut: false);
		}

		protected override void OnUpgrade()
		{
			// 费用 -1（3 → 2）
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
