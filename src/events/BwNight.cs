using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.events
{
	/// <summary>
	/// BW之夜 — 第三层事件。
	/// 全网狂欢之夜，愤怒的夫哥做出选择。
	/// </summary>
	public sealed class BwNight : EventModel
	{
		public override bool IsAllowed(IRunState runState)
		{
			return runState.CurrentActIndex == 2;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			return new EventOption[]
			{
				new EventOption(this, ActWork, "BW_NIGHT.pages.INITIAL.options.WORK"),
				new EventOption(this, ActBailan, "BW_NIGHT.pages.INITIAL.options.BAILAN"),
				new EventOption(this, ActYinren, "BW_NIGHT.pages.INITIAL.options.YINREN"),
			};
		}

		// 默默工作：获得工资 +200（加班费）
		private async Task ActWork()
		{
			SetEventFinished(L10NLookup("BW_NIGHT.pages.WORK.description"));
			if (Owner == null) return;
			var landlord = Owner.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
			if (landlord != null)
			{
				landlord.Salary += 200;
			}
		}

		// 摆烂直播：删除一张牌
		private async Task ActBailan()
		{
			SetEventFinished(L10NLookup("BW_NIGHT.pages.BAILAN.description"));
			if (Owner == null) return;
			var cards = (await CardSelectCmd.FromDeckForRemoval(
				player: Owner,
				prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1),
				filter: c => true)).ToList();
			await CardPileCmd.RemoveFromDeck(cards);
		}

		// 隐忍：获得超巨化药水（搜刮战利品方式，玩家点击领取）
		private async Task ActYinren()
		{
			SetEventFinished(L10NLookup("BW_NIGHT.pages.YINREN.description"));
			if (Owner == null) return;
			var potion = ModelDb.Potion<GigantificationPotion>().ToMutable();
			await RewardsCmd.OfferCustom(Owner, new List<Reward> { new PotionReward(potion, Owner) });
		}
	}
}
