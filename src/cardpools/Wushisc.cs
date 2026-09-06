using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 无视sc — 技能：获得 6 点格挡（升级 9），获得并转化弃牌堆的一张牌
	/// </summary>
	public sealed class Wushisc : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasConvertEffect => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
			{
				new BlockVar(6m, ValueProp.Move)  // 格挡6，升级9
		};

		public Wushisc()
			: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// 获得 6/9 点格挡
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			// 从弃牌堆选一张牌，获得（回手）并转化
			var discard = PileType.Discard.GetPile(Owner);
			if (discard != null && discard.Cards.Count > 0)
			{
				// 关键坑：CardSelectCmd.FromCombatPile 在“弃牌堆恰有 1 张 + 无需手动确认(1,1)”时，
				// 走点卡即取的快捷路径，会把弃牌堆的内部 List（pile.Cards）直接作为返回值，而非副本。
				// 若直接 foreach 它，并在循环里 CardPileCmd.Add(card, PileType.Hand) 回手（会从弃牌堆移除该牌），
				// 就在迭代期间修改了同一个 List，抛 InvalidOperationException: Collection was modified，
				// OnPlay 异常中断，导致整张牌卡在打出牌位（画面顶部）。
				// 修复：先 ToList() 拍成快照再遍历，迭代期间改弃牌堆不再影响枚举。
				// （手牌满时弃牌堆常只剩 1 张，因此更容易复现。）
				var selected = (await CardSelectCmd.FromCombatPile(
						choiceContext,
						discard,
						Owner,
						// 固定选1张且无需手动确认：点击卡牌即完成选择。
						// 避免像（0,1）那样还需额外点一次“确定”（官方涅奥之怒的实现），
						// 对齐官方宇宙冷漠的“点卡即取”体验。
						new CardSelectorPrefs(new LocString("cards", "WUSHISC.selectPrompt"), 1),
						null))
					.ToList();

				foreach (var card in selected)
				{
					await CardPileCmd.Add(card, PileType.Hand);  // 获得回手
					FufuCmd.Toggle(card);  // 转化（仅对夫卡生效）
				}
			}
		}

		protected override void OnUpgrade()
		{
			// 格挡 6 → 9
			DynamicVars.Block.UpgradeValueBy(3m);
		}
	}
}
