using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;
using kyxiaofujiu.monsters;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 正常讲话 — 0费攻击：需要有蛇花小姐在场才能打出。
	/// 对敌人造成 4 点伤害并给予 1 层虚弱；升级 6 点伤害 + 2 层虚弱。
	/// </summary>
	public sealed class Zhengchangjianghua : XiaofujiuCardBase
	{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new DamageVar(4m, ValueProp.Move),
			new PowerVar<WeakPower>(1m)
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

		public Zhengchangjianghua()
			: base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		// 蛇花小姐存在（我方存活、为晓夫九的宠物）才能打出
		private bool HasSheHua()
		{
			var combat = Owner?.Creature?.CombatState;
			if (combat == null) return false;
			return combat.Allies.Any(c => c.Monster is SheHuaXiaoJieMonster && c.PetOwner == Owner && c.IsAlive);
		}

		protected override bool IsPlayable
		{
			get
			{
				// 蛇花小姐不存在则无法打出（卡库/预览跳过判定）
				if (!IsCanonical && !HasSheHua()) return false;
				return base.IsPlayable;
			}
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var target = cardPlay.Target;
			if (target == null) return;
			if (!HasSheHua()) return; // 防御：蛇花已不在则本次无效果

			// 造成伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 给予虚弱
			await PowerCmd.Apply<WeakPower>(choiceContext, target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2m); // 4 → 6
			DynamicVars.Weak.UpgradeValueBy(1m);   // 1 → 2
		}
	}
}
