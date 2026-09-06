using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 投资 — 技能：花费20工资，2回合后获得10-40工资（升级10-50），抽1张牌
	/// </summary>
	public sealed class Touzi : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;

		public override int SalaryCost => 20;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MinGain", 10m),
			new IntVar("MaxGain", 40m)
		};

		public Touzi()
			: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 抽一张牌
			await CardPileCmd.Draw(choiceContext, 1, Owner);

			// 2回合后获得随机工资（用 Apply<T> 创建实例，不能直接 new Power——会触发 DuplicateModelException）
			int min = (int)DynamicVars["MinGain"].BaseValue;
			int max = (int)DynamicVars["MaxGain"].BaseValue;
			var power = await PowerCmd.Apply<TouziPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
			if (power != null)
			{
				power.MinGain = min;
				power.MaxGain = max;
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["MaxGain"].UpgradeValueBy(10m); // 40 → 50
		}
	}
}
