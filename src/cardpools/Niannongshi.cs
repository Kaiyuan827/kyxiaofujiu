using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 念农师 — 夫黄体系测试卡
	/// 随机一张手牌中的夫黑/夫白卡牌转化为夫黄
	/// </summary>
	public sealed class Niannongshi : XiaofujiuCardBase
	{
		public override bool HasBlackEffect => true;
		public override bool HasConvertEffect => true;
		public override FufuState CanonicalFufuState => FufuState.White;
		public override bool HasYellowEffect => true;

		public Niannongshi()
			: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var candidates = PileType.Hand.GetPile(Owner).Cards
				.OfType<XiaofujiuCardBase>()
				.Where(c => c.IsFufuBlack || c.IsFufuWhite)
				.ToList();

			if (candidates.Count > 0)
			{
				var target = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
				if (target != null)
				{
					// 转化为夫黄（同黑白转化一样触发 FufuStateChanged / 遗物 / 创世夫柱等）
					FufuCmd.ConvertToYellow(target);
				}
			}
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
