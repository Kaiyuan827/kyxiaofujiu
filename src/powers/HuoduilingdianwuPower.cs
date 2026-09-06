using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Models;

namespace kyxiaofujiu.powers
{
	public sealed class HuoduilingdianwuPower : PowerModel
	{
		/// <summary>每层回复的生命值（与 AfterCombatEnd 保持一致的常量）。</summary>
		public const int HealPerLayer = 5;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;  // ✅ 支持叠加

		// 只用于显示的变量：{TotalHeal} = 5 × Amount(层数)。Amount 语义仍是"层数"，参与实际计算时直接用它。
		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new AmountProductVar("TotalHeal", p => ((HuoduilingdianwuPower)p).Amount * HealPerLayer)
		};

		public override async Task AfterCombatEnd(CombatRoom room)
		{
			// 任何战斗（普通/精英/Boss）结束后都触发
			if (room.Encounter.RoomType == RoomType.Event) return;
			if (Owner.IsDead) return;

			var player = Owner.Player;
			if (player == null) return;

			// 每层触发一次效果（Amount = 层数）
			for (int i = 0; i < Amount; i++)
			{
				Flash();
				await CreatureCmd.Heal(Owner, HealPerLayer);  // 每层回复 5 点生命

				var deck = player.Deck;
				var upgradableCards = deck.Cards.Where(c => c.IsUpgradable).ToList();
				if (upgradableCards.Count > 0)
				{
					var card = player.RunState.Rng.CombatCardSelection.NextItem(upgradableCards);
					if (card != null)
					{
						CardCmd.Upgrade(card);
					}
				}
			}
		}
	}
}
