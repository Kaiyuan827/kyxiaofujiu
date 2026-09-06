using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 农夫蛇豹 — 能力：花费 150 工资。获得壁垒，选择一张手牌转化为夫黄，征婚4，获得22点格挡
	/// </summary>
	public sealed class Nongfushebao : XiaofujiuCardBase
	{
	public override bool GainsBlock => true;
		public override bool HasMarriageEffect => true;
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.Black;
		public override bool HasYellowEffect => true;

		// 打出需要花费 150 工资（永久跨战斗保留资源）
		public override int SalaryCost => 150;

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new BlockVar(22m, ValueProp.Move),
			new IntVar("MarryCount", 4m)
		};

		public Nongfushebao()
			: base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

			// 获得壁垒（格挡不再消失）
			await PowerCmd.Apply<BarricadePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

			// 选择一张手牌（夫黑/夫白）转化为夫黄
			var selected = await CardSelectCmd.FromHand(
				choiceContext,
				Owner,
				new CardSelectorPrefs(new LocString("cards", "NONGFUSHEBAO.selectPrompt"), 0, 1),
				c => c is XiaofujiuCardBase fc && (fc.IsFufuBlack || fc.IsFufuWhite),
				this);

			foreach (var card in selected)
			{
				if (card is XiaofujiuCardBase fc)
				{
					FufuCmd.ConvertToYellow(fc);
				}
			}

			// 征婚4
			await ZhengHunCmd.Marry(choiceContext, Owner, (int)DynamicVars["MarryCount"].BaseValue);

			// 获得22点格挡
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1); // 费用 1 → 0
		}
	}
}
