using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// nosl高手 — 战斗开始时对所有敌人造成X点伤害，之后X+1。
	/// 累计值用普通字段（非 [SavedProperty]）：SL 后会清空回默认 1。
	/// ⚠ 多人模式禁用：该非序列化累计值在双端各自累计会不一致 → 战斗动作不同步导致报错卡死。
	///   修复需持久化该值，但那会让“SL 清空”机制失效，故改为多人模式不发放。
	/// </summary>
	public sealed class NoSlGaoShou : RelicModel
	{
		private int _damage = 1;  // 累计伤害值（SL 后清空）

		public override RelicRarity Rarity => RelicRarity.Common;

		public override bool ShowCounter => true;
		public override int DisplayAmount => _damage;

		// 多人模式不发放该遗物（奖励/商店/Neow 均不会出现）
		public override bool IsAllowed(IRunState runState)
		{
			if (runState.Players.Count > 1) return false;
			return base.IsAllowed(runState);
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new DamageVar(1m, ValueProp.Unpowered)
		};

		// 战斗开始（玩家第一回合）：对所有敌人造成当前伤害，然后伤害+1
		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner) return;
			if (Owner.PlayerCombatState.TurnNumber > 1) return;  // 仅战斗开始第一回合触发

			// 多人兜底：即使旧档已持有也在多人下不生效（防双端不同步）
			if (Owner.Creature?.CombatState?.Players.Count > 1) return;

			Flash();
			await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature);

			_damage++;
			if (DynamicVars?["Damage"] is DamageVar d) d.BaseValue = _damage;
			InvokeDisplayAmountChanged();
		}
	}
}
