using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.powers;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Xiatounan : XiaofujiuCardBase
	{
		public override bool HasMarriageEffect => true;
		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new IntVar("StrengthGain", 2m),
			new IntVar("DexterityGain", 2m)
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<StrengthPower>());
				tips.Add(HoverTipFactory.FromPower<DexterityPower>());
				return tips;
			}
		}

		public Xiatounan()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

			int strength = (int)DynamicVars["StrengthGain"].BaseValue;
			int dexterity = (int)DynamicVars["DexterityGain"].BaseValue;

			// 获得力量
			await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strength, Owner.Creature, this);
			// 获得敏捷
			await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, dexterity, Owner.Creature, this);

			// 2回合内不能征婚（层数为2）
			await PowerCmd.Apply<NoMarryPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
			// ✅ 强制本回合结束就扣除一层
			var noMarry = Owner.Creature.GetPower<NoMarryPower>();
			if (noMarry != null)
			{
				noMarry.SkipNextDurationTick = false;
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["StrengthGain"].UpgradeValueBy(1m);
			DynamicVars["DexterityGain"].UpgradeValueBy(1m);
		}
	}
}
