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
	public sealed class Jimi : XiaofujiuCardBase
	{
		// ✅ 添加动态变量用于升级显示
		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new IntVar("WeakAmount", 3m)  // 平衡性调整：易伤→虚弱
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<WeakPower>());
				return tips;
			}
		}

		public Jimi()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

			// 施加能力
			await PowerCmd.Apply<JimiPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

			// 平衡性调整：易伤改为虚弱（升级后自动变化）
			int weakAmount = (int)DynamicVars["WeakAmount"].BaseValue;
			await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, weakAmount, Owner.Creature, this);

			// 强制本回合结束扣除虚弱
			var weak = Owner.Creature.GetPower<WeakPower>();
			if (weak != null)
			{
				weak.SkipNextDurationTick = false;
			}
		}

		protected override void OnUpgrade()
		{
			// 虚弱3→2
			var weakAmount = (IntVar)DynamicVars["WeakAmount"];
			weakAmount.UpgradeValueBy(-1m);
		}
	}
}
