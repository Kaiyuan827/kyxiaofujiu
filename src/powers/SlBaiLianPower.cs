using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.monsters;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// sl百连：血量低于当前阈值(初始50)时恢复满血，阈值-10，并召唤一只敌方蛇花
	/// Amount 保存当前触发阈值（每次触发 -10）；用 Counter 让右下角角标实时显示阈值。
	/// </summary>
	public sealed class SlBaiLianPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner) return;
			if (Owner.IsDead || result.WasTargetKilled) return; // 直接打死不触发
			if (Owner.CurrentHp >= Amount) return; // 未低于阈值
			if (Amount <= 0) return;

			// 阈值-10
			SetAmount(Amount - 10);

			// 恢复满血
			await CreatureCmd.Heal(Owner, Owner.MaxHp - Owner.CurrentHp);

			// 召唤一只敌方蛇花到空闲槽位并标记为爪牙
			string slot = CombatState.Encounter.GetNextSlot(CombatState);
			var minion = await CreatureCmd.Add<EnemySheHua>(CombatState, string.IsNullOrEmpty(slot) ? null : slot);
			await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), minion, 1m, Owner, null);
		}
	}
}
