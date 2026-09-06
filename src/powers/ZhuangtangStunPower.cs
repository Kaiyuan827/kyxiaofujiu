using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 装唐的延时眩晕（挂在玩家身上，不影响敌人意图树）：
	/// 本轮敌人正常行动，玩家回合开始时眩晕所有敌人 → 下一轮敌人全部被击晕
	/// </summary>
	public sealed class ZhuangtangStunPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.None;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
		{
			HoverTipFactory.Static(StaticHoverTip.Stun)
		};

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (Owner == null || Owner.IsDead) return;

			var combatState = Owner.CombatState;
			if (combatState == null) return;

			// 玩家回合开始：眩晕所有存活敌人 → 下一轮敌人全部被击晕
			var enemies = combatState.Creatures
				.Where(c => c.Side == CombatSide.Enemy && c.IsAlive)
				.ToList();
			foreach (var enemy in enemies)
			{
				await CreatureCmd.Stun(enemy);
			}

			await PowerCmd.Remove(this);
		}
	}
}
