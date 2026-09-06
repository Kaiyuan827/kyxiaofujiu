using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 苦苦苦 — 技能 0费：获得40工资（升级50），3回合内不能征婚。初始夫黑
	/// </summary>
	public sealed class Kukuku : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.Black;  // 初始夫黑

		// 获得工资 + 涉及征婚机制（不能征婚），hover 显示工资/征婚说明
		public override bool ShowSalaryHover => true;
		public override bool HasMarriageEffect => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("SalaryGain", 40m)
		};

		public Kukuku()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)  // 改0费
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 获得工资（统一入口，含爆米加成）
			SalaryCmd.Gain(Owner, (int)DynamicVars["SalaryGain"].BaseValue);

			// 3回合内不能征婚（参考虾头楠：层数=回合数，强制本回合结束扣一层）
			await PowerCmd.Apply<NoMarryPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
			var noMarry = Owner.Creature.GetPower<NoMarryPower>();
			if (noMarry != null)
			{
				noMarry.SkipNextDurationTick = false;
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["SalaryGain"].UpgradeValueBy(10m); // 40 → 50
		}
	}
}
