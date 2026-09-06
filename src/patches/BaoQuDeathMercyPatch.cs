using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.monsters;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.patches
{
	/// <summary>
	/// 豹区（BaoQu Boss）战斗开始时，给每位玩家挂上隐藏的死亡豁免能力
	/// （死亡时保留一滴血并弹窗询问是否接受死亡）。
	/// </summary>
	[HarmonyPatch(typeof(CombatManager), "SetUpCombat")]
	public static class BaoQuDeathMercy_SetupCombat_Patch
	{
		static void Postfix(CombatState state)
		{
			try
			{
				if (state == null) return;
				// 是否为豹区 Boss 战（敌方含 BaoQu）
				bool isBaoQuFight = state.Enemies.Any(c => c.Monster is BaoQu);
				if (!isBaoQuFight) return;

				foreach (Player player in state.Players)
				{
					if (player?.Creature == null) continue;
					// 已有则跳过（防重复叠加）
					if (player.Creature.GetPower<BaoQuDeathMercyPower>() != null) continue;
					_ = TaskHelper.RunSafely(ApplyMercyAsync(player));
				}
			}
			catch (Exception e)
			{
				Log.Error($"[BaoQuDeathMercy_SetupCombat_Patch] 失败: {e.Message}");
			}
		}

		private static async Task ApplyMercyAsync(Player player)
		{
			try
			{
				await PowerCmd.Apply<BaoQuDeathMercyPower>(
					new ThrowingPlayerChoiceContext(), player.Creature, 1m, null, null);
			}
			catch (Exception e)
			{
				Log.Error($"[BaoQuDeathMercy_SetupCombat_Patch] 施加能力失败: {e.Message}");
			}
		}
	}
}
