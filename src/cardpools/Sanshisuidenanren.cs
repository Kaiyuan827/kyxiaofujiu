using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.core;
using kyxiaofujiu.powers;

namespace kyxiaofujiu.cardpools
{
	/// <summary>
	/// 三十岁的男人 — 能力：当你征婚时，获得 2 倍征婚值的格挡。升级：费用 -1。
	/// </summary>
	public sealed class Sanshisuidenanren : XiaofujiuCardBase
	{
		public override FufuState CanonicalFufuState => FufuState.White;  // 初始夫白

		public Sanshisuidenanren()
			: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)  // 罕见
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			// Amount = 格挡倍率（每张基础 2 倍，可叠加：2张=4倍）
			await PowerCmd.Apply<SanshisuidenanrenPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);  // 费用 1 → 0
		}
	}
}
