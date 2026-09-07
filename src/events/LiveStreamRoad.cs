using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using kyxiaofujiu.cardpools;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.events
{
	public sealed class LiveStreamRoad : EventModel
	{
		public override bool IsAllowed(IRunState runState)
		{
			// 只允许在第一层（Underdocks）触发
			return runState.CurrentActIndex == 0;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			return new EventOption[]
			{
				// 鼠标悬停在选项上可查看对应遗物/卡牌的详细信息
				new EventOption(this, ActTech, "LIVE_STREAM_ROAD.pages.INITIAL.options.TECH", GetTechHoverTips()),
				new EventOption(this, ActYayi, "LIVE_STREAM_ROAD.pages.INITIAL.options.YAYI", GetRelicHoverTips<WeddingRing>()),
				new EventOption(this, ActYourself, "LIVE_STREAM_ROAD.pages.INITIAL.options.YOURSELF", GetRelicHoverTips<KaRelic>()),
			};
		}

		// 技术流主播：尖塔糕手（遗物）+ 长考（卡牌）的悬浮信息
		private static IEnumerable<IHoverTip> GetTechHoverTips()
		{
			var tips = new List<IHoverTip>();
			tips.AddRange(ModelDb.Relic<MyCustomRelic>().HoverTips);

			// 卡牌用完整卡牌预览（CardHoverTip）：费用/名称/描述/关键字都正常渲染，
			// 动态变量（{Block} 等）会显示实际数值，像游戏内卡牌 hover 一样
			tips.Add(HoverTipFactory.FromCard<Changkao>());
			return tips;
		}

		// 遗物悬浮信息（HoverTips 已含遗物自身标题+描述）
		private static IEnumerable<IHoverTip> GetRelicHoverTips<T>() where T : RelicModel
		{
			return ModelDb.Relic<T>().HoverTips;
		}

		// 事件固定遗物：已拥有则不再重复发放（避免重复遗物），改为金币替代补偿
		private async Task GiveRelicIfMissing<T>(int goldCompensation = 50) where T : RelicModel
		{
			if (base.Owner.Relics.OfType<T>().Any())
				await PlayerCmd.GainGold(goldCompensation, base.Owner);
			else
				await RelicCmd.Obtain<T>(base.Owner);
		}

		private async Task ActTech()
		{
			SetEventFinished(L10NLookup("LIVE_STREAM_ROAD.pages.TECH.description"));
			// 尖塔糕手 + 长考
			await GiveRelicIfMissing<MyCustomRelic>();
			// 用 CreateCard 创建带 owner 的卡（ModelDb.Card 的 canonical 无 owner，CardPileCmd.Add 会抛异常）
			var changkao = base.Owner.RunState.CreateCard<Changkao>(base.Owner);
			await CardPileCmd.Add(changkao, PileType.Deck);
		}

		private async Task ActYayi()
		{
			SetEventFinished(L10NLookup("LIVE_STREAM_ROAD.pages.YAYI.description"));
			await GiveRelicIfMissing<WeddingRing>();
		}

		private async Task ActYourself()
		{
			SetEventFinished(L10NLookup("LIVE_STREAM_ROAD.pages.YOURSELF.description"));
			await GiveRelicIfMissing<KaRelic>();
		}
	}
}
