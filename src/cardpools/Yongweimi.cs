using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 用萎靡 — 技能：场上所有生物（含晓夫九、蛇花小姐、所有敌人）失去力量，消耗
	/// 策略：先打出本牌再召唤蛇花小姐，可避免蛇花小姐受负面效果
	/// </summary>
	public sealed class Yongweimi : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new DynamicVar("StrengthLoss", 6m)
		};

		public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

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

		public Yongweimi()
			: base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

			int loss = (int)DynamicVars["StrengthLoss"].BaseValue;

			// 场上所有生物（含晓夫九、蛇花小姐、所有敌人）失去力量
			var creatures = Owner.Creature.CombatState.Creatures
				.Where(c => c.IsAlive)
				.ToList();

			foreach (var creature in creatures)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, creature, -loss, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["StrengthLoss"].UpgradeValueBy(2m);
		}
	}
}
