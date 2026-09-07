using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 豹区（BaoQu）Boss 战的死亡豁免（隐藏，不显示图标）：
	/// 玩家即将被打死时阻止死亡、回复满血，并弹窗询问"是否接受死亡？"
	/// - 绿色按钮【回到主菜单】：存档退出、可读档重打豹区（安全 SL）
	/// - 红色按钮【接受死亡】：对自己造成满血伤害 → 真正死亡（走正常死亡流程）
	/// </summary>
	public sealed class BaoQuDeathMercyPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		// 隐藏：不在能力栏/生物头顶显示
		protected override bool IsVisibleInternal => false;

		// 玩家已选择"接受死亡"（或本能力已放行），之后不再拦截
		private bool _allowRealDeath;

		public override bool ShouldReceiveCombatHooks => true;

		public override bool ShouldDie(Creature creature)
		{
			// 只拦自己（Owner = 挂载本能力的玩家 creature）
			if (creature != Owner) return true;
			// 已放行（玩家选择接受死亡 / 再次濒死不再打扰）
			if (_allowRealDeath) return true;
			// 阻止这次死亡（成为 preventer，随后触发 AfterPreventingDeath）
			return false;
		}

		public override async Task AfterPreventingDeath(Creature creature)
		{
			if (creature != Owner) return;
			try
			{
				// 先标记放行：若之后玩家再次濒死（如多段攻击第二段），不再反复弹窗/拦截
				_allowRealDeath = true;

				// 回复满血：把玩家从濒死救回满血（防止 Kill 流程因 IsDead 递归）
				if (creature.IsDead || creature.CurrentHp <= 0)
				{
					await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp, playAnim: false);
				}

				// 弹出"是否接受死亡？"确认框（模态，等待玩家选择）
				// 注意按钮颜色语义：绿色=回到主菜单（安全 SL），红色=接受死亡（投降）。
				// 官方 NVerticalPopup 的 Yes 位=绿、No 位=红，WaitForConfirmation 返回 true=按了绿色。
				bool safeSl = await AskAcceptDeathAsync();

				// 当前仍处于 Kill 递归链中，不能在此直接自杀/切场景；
				// 先把选择结果交给独立异步任务，等本结算链结束后执行。
				if (safeSl)
				{
					// 绿色（回到主菜单）：返回主菜单（等同暂停菜单"保存并退出"，可读档重打豹区）
					_ = TaskHelper.RunSafely(QuitToMainMenuAsync());
				}
				else
				{
					// 红色（接受死亡）：对自己造成满血伤害（走正常死亡流程）
					_ = TaskHelper.RunSafely(ExecuteAcceptedDeathAsync(creature));
				}
			}
			catch (Exception e)
			{
				Log.Error($"[BaoQuDeathMercyPower.AfterPreventingDeath] 失败: {e.Message}");
			}
		}

		// 玩家接受死亡：等本结算链结束后，对自己造成满血伤害 → 触发正常死亡流程（Game Over）
		private static async Task ExecuteAcceptedDeathAsync(Creature creature)
		{
			try
			{
				await System.Threading.Tasks.Task.Yield();
				if (creature == null || creature.IsDead) return;
				if (creature.CombatState == null) return;
				// 满血伤害：对满血玩家造成等于其最大生命的伤害，必然致死
				decimal fullHpDamage = Math.Max(creature.MaxHp, 1m);
				await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), creature, fullHpDamage,
					ValueProp.Unblockable | ValueProp.Unpowered, null, null, null);
			}
			catch (Exception e)
			{
				Log.Error($"[BaoQuDeathMercyPower] 执行死亡失败: {e.Message}");
			}
		}

		private static async Task QuitToMainMenuAsync()
		{
			try
			{
				await System.Threading.Tasks.Task.Yield();
				if (NGame.Instance != null)
				{
					await NGame.Instance.ReturnToMainMenu();
				}
			}
			catch (Exception e)
			{
				Log.Error($"[BaoQuDeathMercyPower] 回主菜单失败: {e.Message}");
			}
		}

		/// <summary>
		/// 弹出一个"是否接受死亡？"的是/否确认框。
		/// 返回 true = 按了绿色（回到主菜单/安全 SL）；false = 按了红色（接受死亡）。
		/// 任何异常/无法弹窗时按"接受死亡"兜底，避免玩家永不死/战斗卡死。
		/// </summary>
		private static async Task<bool> AskAcceptDeathAsync()
		{
			try
			{
				var popup = NGenericPopup.Create();
				if (popup == null || NModalContainer.Instance == null)
				{
					// 极端兜底：无法弹窗时按"接受死亡"处理（不无限阻止）
					return true;
				}
				NModalContainer.Instance.Add(popup);
				return await popup.WaitForConfirmation(
					new LocString("powers", "BAOQU_DEATH_ASK.body"),
					new LocString("powers", "BAOQU_DEATH_ASK.title"),
					// 红按钮（No 位）= 接受死亡；绿按钮（Yes 位）= 回到主菜单（安全 SL）
					new LocString("powers", "BAOQU_DEATH_ASK.yes"),
					new LocString("powers", "BAOQU_DEATH_ASK.no"));
			}
			catch (Exception e)
			{
				Log.Error($"[BaoQuDeathMercyPower] 弹窗失败，按接受死亡处理: {e.Message}");
				return true;
			}
		}
	}
}
