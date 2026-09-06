using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace kyxiaofujiu.powers
{
	public sealed class FuhuangscPower : PowerModel
	{
		private bool _shouldApplyVulnerable = false;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
		{
			HoverTipFactory.FromPower<VulnerablePower>()
		};

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner) return;
			// 只认攻击伤害：须有攻击者且为 Powered 攻击（征婚/失去生命等 dealer=null 或 Unpowered 不触发）
			if (dealer == null) return;
			if (!props.IsPoweredAttack()) return;
			if (result.UnblockedDamage > 0)
			{
				MarkDamaged();
			}
		}

		// 置位"本回合已受到攻击"（供蛇花小姐受伤时转发调用）
		public void MarkDamaged()
		{
			_shouldApplyVulnerable = true;
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (side == CombatSide.Enemy)
			{
				if (_shouldApplyVulnerable)
				{
					await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner, 1m, Owner, null);
					
					// 强制易伤在下一个敌人回合结束时扣除
					var vuln = Owner.GetPower<VulnerablePower>();
					if (vuln != null)
					{
						vuln.SkipNextDurationTick = false;
					}
				}
				await PowerCmd.Remove(this);
			}
		}

		public override bool ShouldReceiveCombatHooks => true;
	}
}
