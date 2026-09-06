using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using kyxiaofujiu.powers;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.commands
{
	/// <summary>
	/// 工资操作统一入口：获得工资（含爆米加成）
	/// 所有"获得工资"的卡牌/能力都应调用此方法，而不是直接改 Salary。
	/// </summary>
	public static class SalaryCmd
	{
		/// <summary>
		/// 获得工资（触发爆米：每层额外 +15 工资）
		/// </summary>
		public static void Gain(Player? player, int amount)
		{
			if (player == null || amount <= 0) return;
			var landlord = player.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
			if (landlord == null) return;

			// 爆米：获得工资时每层额外 +15 工资
			var baomi = player.Creature?.GetPower<BaomiPower>();
			if (baomi != null)
			{
				amount += baomi.Amount;
			}

			landlord.Salary += amount;

			// 差偏认知：战斗中获得工资时征婚（每层征婚 MarryCount 次）
			if (CombatManager.Instance.IsOverOrEnding) return;
			var cp = player.Creature?.GetPower<ChapianrenzhiPower>();
			if (cp != null)
			{
				TaskHelper.RunSafely(cp.TriggerMarry(player));
			}
		}
	}
}
