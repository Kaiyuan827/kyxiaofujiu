using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using MegaCrit.Sts2.Core.Logging;  // ✅ 添加调试日志
using kyxiaofujiu.utils;

namespace kyxiaofujiu.powers
{
	public sealed class ChapianrenzhiPower : PowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipHelper.MarriageTips;

		// Amount 语义：每次获得工资时征婚的总次数（由 Chapianrenzhi 卡按 MarryCount 施加，叠加时累加）。

		// ✅ 供 Harmony 补丁调用的触发方法
		public async Task TriggerMarry(Player player)
		{
			if (player != Owner.Player) return;
			if (Amount <= 0) return;

			Log.Info($"=== 差偏认知 触发征婚，每次征婚次数: {Amount} ===");
			Flash();

			await ZhengHunCmd.Marry(new ThrowingPlayerChoiceContext(), player, Amount);
		}
	}
}
