using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 牛马 — 技能：获得格挡。回合结束时若此牌在手牌中，获得工资。
	/// 期望玩家将其转变为夫黑（夫黑带 Retain 保留在手牌），从而每回合结束时持续获得工资。
	/// </summary>
	public sealed class Niuma : XiaofujiuCardBase
	{
		public override bool ShowSalaryHover => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		public override bool GainsBlock => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new BlockVar(8m, ValueProp.Move),
			new IntVar("SalaryGain", 10m)
		};

		// 需要接收战斗钩子，才能在回合结束时检查自己是否在手牌中
		public override bool ShouldReceiveCombatHooks => true;

		public Niuma()
			: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)  // 普通
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}

		// 回合结束时若此牌在手牌中则获得工资。
		// 不移动卡牌：夫黑（Retain）会在弃牌阶段保留在手牌，从而每回合都触发。
		public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side != CombatSide.Player) return;
			if (Owner == null || Owner.Creature == null || Owner.Creature.IsDead) return;

			// 仅当此牌当前在手牌中才生效（弃牌堆/牌堆中的不生效）
			var hand = PileType.Hand.GetPile(Owner);
			if (hand == null || !hand.Cards.Contains(this)) return;

			// 回合结束时若在手牌则获得工资（统一入口，含爆米加成）
			SalaryCmd.Gain(Owner, (int)DynamicVars["SalaryGain"].BaseValue);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3m);          // 8 → 11
			DynamicVars["SalaryGain"].UpgradeValueBy(5m);  // 10 → 15
		}
	}
}
