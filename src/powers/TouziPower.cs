using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using kyxiaofujiu.commands;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 投资 — 2回合后获得随机工资（层数归零时触发并移除）
	/// </summary>
	public sealed class TouziPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		private int _minGain;
		private int _maxGain;

		// 随机工资范围（声明为动态变量，smartDescription 的 {MinGain}-{MaxGain} 才能正常显示）
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new IntVar("MinGain", 10m),
			new IntVar("MaxGain", 40m)
		};

		public int MinGain
		{
			get => _minGain;
			set
			{
				_minGain = value;
				if (DynamicVars?["MinGain"] is IntVar v) v.BaseValue = value;
			}
		}

		public int MaxGain
		{
			get => _maxGain;
			set
			{
				_maxGain = value;
				if (DynamicVars?["MaxGain"] is IntVar v) v.BaseValue = value;
			}
		}

		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side != CombatSide.Player) return;
			if (Owner == null || Owner.IsDead) return;

			// 打出回合不算，下个回合开始扣到1，下下个回合开始触发
			if (Amount <= 1)
			{
				// 到期：获得随机工资（统一入口，含爆米加成）
				int gain = Owner.Player.RunState.Rng.CombatCardSelection.NextInt(MinGain, MaxGain + 1);
				SalaryCmd.Gain(Owner.Player, gain);

				// 气泡提示
				try
				{
					var bubble = NSpeechBubbleVfx.Create($"投资回报：+{gain} 工资！", Owner, 2.5);
					NCombatRoom.Instance?.CombatVfxContainer?.AddChild(bubble);
				}
				catch (System.Exception) { }
				await PowerCmd.Remove(this);
			}
			else
			{
				await PowerCmd.TickDownDuration(this);
			}
		}
	}
}
