using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;


namespace kyxiaofujiu.cardpools
{
	public sealed class Fuheishuohua : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasBlackEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new BlockVar(6m, ValueProp.Move)  // 平衡性调整：格挡 5→6
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null)
				{
					tips.AddRange(baseTips);
				}
				tips.AddRange(HoverTipHelper.FuheiPlayTips);
				return tips;
			}
		}

		public Fuheishuohua()
			: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

			var selected = await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(
					new LocString("cards", "FUHEISHUOHUA.selectPrompt"),
					1
				),
				c => c is XiaofujiuCardBase baseCard && baseCard.IsFufuBlack,
				this
			);

			var card = selected.FirstOrDefault();
			if (card == null)
			{
				Log.Info("Fuheishuohua: 未选择夫黑卡牌");
				return;
			}

			await FufuBlackCardPlay.Play(card, choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3m); // 格挡 6 → 9（平衡性调整：升级改格挡，不再减费）
		}
	}
}
