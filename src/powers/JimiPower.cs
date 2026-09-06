using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	public sealed class JimiPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			if (side != CombatSide.Player) return;
			if (Owner.IsDead) return;

			// ✅ 修正：层数 × 1点能量
			await PlayerCmd.GainEnergy(1m * Amount, Owner.Player);

			// ✅ 修正：每层播放一次音效，间隔0.1秒
			for (int i = 0; i < Amount; i++)
			{
				CustomAudioManager.PlaySfx("res://audio/jimi.mp3", 10f);  // 音量+10dB
				if (i < Amount - 1)
				{
					await Cmd.Wait(0.1f);
				}
			}
		}
	}
}
