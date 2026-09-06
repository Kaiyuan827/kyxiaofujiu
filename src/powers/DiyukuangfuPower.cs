using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	/// <summary>
	/// 地狱狂夫：每当你抽到夫黑时，将其打出（强制打出，自动选目标）。
	/// 参考游戏 HellraiserPower（AfterCardDrawnEarly 钩子 + 自动打出）。
	/// </summary>
	public sealed class DiyukuangfuPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.FuheiPlayTips;

		public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			// 只处理自己抽到的夫黑（card.Owner 是玩家，Owner 是本能力所在 Creature）
			if (card.Owner?.Creature != Owner) return;
			if (card is not XiaofujiuCardBase fufuCard || !fufuCard.IsFufuBlack) return;

			// 延迟 0.5 秒再打出，让玩家看清抽到了什么
			await Cmd.CustomScaledWait(0.5f, 0.5f);

			// 夫黑强制打出：FufuBlackCardPlay 自动选目标并走夫黑打出机制
			await FufuBlackCardPlay.Play(fufuCard, choiceContext);
		}
	}
}
