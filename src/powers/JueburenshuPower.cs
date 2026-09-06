using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace kyxiaofujiu.powers
{
	public sealed class JueburenshuPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;  // ✅ 改为 Counter

		public override bool ShouldDieLate(Creature creature)
		{
			if (creature != Owner) return true;
			// 只要还有层数就阻止死亡
			return Amount <= 0;
		}

		public override async Task AfterPreventingDeath(Creature creature)
		{
			Flash();
			await CreatureCmd.Heal(creature, 10m);
			await CreatureCmd.GainMaxHp(creature, 2m);

			// 减少一层
			await PowerCmd.Decrement(this);
		}

		// 敌人回合结束后，如果还有未使用的层数，移除（本回合过期）
		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side == CombatSide.Enemy && Amount > 0)
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}
