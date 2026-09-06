using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;

namespace kyxiaofujiu.epochs
{
	public sealed class Xiaofujiu2Epoch : EpochModel
	{
		public override string Id => "XIAOFUJIU2_EPOCH";
		public override EpochEra Era => EpochEra.Blight1;
		public override int EraPosition => 1;
		public override string StoryId => "Xiaofujiu";

		// 暂时返回空列表，不解锁任何卡牌（后续可添加）
		public static List<CardModel> Cards => new List<CardModel>();

		public override string UnlockText => "解锁晓夫九的第二幕征程。";

		public override void QueueUnlocks()
		{
			// 暂不解锁任何内容
		}
	}
}
