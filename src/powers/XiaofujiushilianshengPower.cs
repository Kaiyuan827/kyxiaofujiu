using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace kyxiaofujiu.powers
{
	public sealed class XiaofujiushilianshengPower : PowerModel
	{
		private bool _hasTriggeredThisTurn;

		// ✅ 改为 Counter，支持叠加
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		// 玩家回合开始时重置触发标记
		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side == CombatSide.Player)
			{
				_hasTriggeredThisTurn = false;
			}
		}

		// 修改生命损失，阈值 = Amount（总累加值）
		public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner) return amount;
			if (_hasTriggeredThisTurn) return amount;

			// Amount 是所有施加重数的总和（5+7+5=17）
			int totalThreshold = Amount;

			if (amount < totalThreshold && amount > 0)
			{
				_hasTriggeredThisTurn = true;
				Flash();
				return 0m;
			}

			return amount;
		}
	}
}
