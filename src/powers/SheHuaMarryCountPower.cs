using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.monsters;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 蛇花小姐征婚层数显示 — 图标数字与悬浮提示显示当前累计征婚层数
	/// </summary>
	public sealed class SheHuaMarryCountPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		private static readonly LocString _title = new LocString("powers", "SHE_HUA_MARRY_COUNT_POWER.title");

		// 蛇花小姐受到攻击伤害时 → 主人（玩家）身上的夫黄SC 也视为受到攻击（下回合易伤）
		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner) return;
			if (dealer == null) return;
			if (!props.IsPoweredAttack()) return;
			if (result.UnblockedDamage <= 0) return;
			if (Owner.IsDead) return;

			var fuhuang = Owner.PetOwner?.Creature?.GetPower<FuhuangscPower>();
			fuhuang?.MarkDamaged();
		}

		// 图标上的数字 = 当前征婚层数（而非固定层数1）
		public override int DisplayAmount
		{
			get
			{
				return (Owner?.Monster as SheHuaXiaoJieMonster)?.MarryCount ?? 0;
			}
		}

		/// <summary>
		/// 征婚值变化后刷新图标显示（Upgrade/IncrementMarryCountOnly 只改 _marryCount，
		/// 不触发 DisplayAmountChanged → 图标不刷新会与真实征婚值不一致）
		/// </summary>
		public void RefreshDisplay()
		{
			InvokeDisplayAmountChanged();
		}

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				int count = (Owner?.Monster as SheHuaXiaoJieMonster)?.MarryCount ?? 0;
				yield return new HoverTip(_title, $"当前征婚层数：{count}");
				foreach (var tip in HoverTipHelper.MarriageTips) yield return tip;
			}
		}
	}
}
