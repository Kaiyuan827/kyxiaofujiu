using System.Collections.Generic;
using System.Linq;
using kyxiaofujiu.core;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;   // Player
using MegaCrit.Sts2.Core.Entities.Cards;     // CardPile, PileType

namespace kyxiaofujiu.commands
{
	public static class FufuCmd
	{
		/// <summary>
		/// 将夫白转为夫黑（施加封印）
		/// </summary>
		public static void Seal(CardModel card)
		{
			if (card is XiaofujiuCardBase fufuCard && fufuCard.IsFufuWhite)
			{
				fufuCard.Seal();
			}
		}

		/// <summary>
		/// 将夫黑转为夫白（解除封印）
		/// </summary>
		public static void Unseal(CardModel card)
		{
			if (card is XiaofujiuCardBase fufuCard && fufuCard.IsFufuBlack)
			{
				fufuCard.Unseal();
			}
		}

		/// <summary>
		/// 切换夫黑夫白状态（黑↔白）
		/// </summary>
		public static void Toggle(CardModel card)
		{
			if (card is XiaofujiuCardBase fufuCard && fufuCard.IsFufu)
			{
				fufuCard.ToggleFufu();
			}
		}

		/// <summary>
		/// 将夫黑/夫白转化为夫黄
		/// </summary>
		public static void ConvertToYellow(CardModel card)
		{
			if (card is XiaofujiuCardBase fufuCard && (fufuCard.IsFufuBlack || fufuCard.IsFufuWhite))
			{
				fufuCard.ConvertToYellow();
			}
		}

		/// <summary>
		/// 交换两张卡牌的夫黑夫白状态
		/// </summary>
		public static void Swap(CardModel card1, CardModel card2)
		{
			if (card1 is not XiaofujiuCardBase f1) return;
			if (card2 is not XiaofujiuCardBase f2) return;

			var temp = f1.FufuState;
			f1.FufuState = f2.FufuState;
			f2.FufuState = temp;
		}

		/// <summary>
		/// 获取手牌中夫黑卡牌数量
		/// </summary>
		public static int CountBlackInHand(Player player)
		{
			return PileType.Hand.GetPile(player).Cards
				.OfType<XiaofujiuCardBase>()
				.Count(c => c.IsFufuBlack);
		}

		/// <summary>
		/// 获取手牌中夫白卡牌数量
		/// </summary>
		public static int CountWhiteInHand(Player player)
		{
			return PileType.Hand.GetPile(player).Cards
				.OfType<XiaofujiuCardBase>()
				.Count(c => c.IsFufuWhite);
		}
	}
}
