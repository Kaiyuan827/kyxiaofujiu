using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.core;
using kyxiaofujiu.monsters;


namespace kyxiaofujiu.cardpools
{
	public sealed class Caokongyaokongqi : XiaofujiuCardBase
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<ArtifactPower>());
				return tips;
			}
		}

		public Caokongyaokongqi()
			: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)  // 改1费
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

			// 获得1层人工制品
			await PowerCmd.Apply<ArtifactPower>(
				choiceContext,
				Owner.Creature,
				1m,
				Owner.Creature,
				this
			);

			// 隐藏彩蛋：在豹区战斗中使用 → 给予豹区 99 层易伤（不写入卡面描述）
			try
			{
				var combat = Owner?.Creature?.CombatState;
				if (combat == null) return;
				var baoqus = combat.Enemies.Where(c => c.IsAlive && c.Monster is BaoQu).ToList();
				foreach (var baoqu in baoqus)
				{
					await PowerCmd.Apply<VulnerablePower>(choiceContext, baoqu, 99m, Owner.Creature, this);
				}
			}
			catch (Exception e)
			{
				MegaCrit.Sts2.Core.Logging.Log.Error($"[Caokongyaokongqi] 彩蛋触发失败: {e.Message}");
			}
		}

		protected override void OnUpgrade()
		{
			// 升级后变固有
			AddKeyword(CardKeyword.Innate);
		}
	}
}
