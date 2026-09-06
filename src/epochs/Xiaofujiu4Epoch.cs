using MegaCrit.Sts2.Core.Timeline;

namespace kyxiaofujiu.epochs
{
	public sealed class Xiaofujiu4Epoch : EpochModel
	{
		public override string Id => "XIAOFUJIU4_EPOCH";
		public override EpochEra Era => EpochEra.Blight1;
		public override int EraPosition => 6;
		public override string StoryId => "Xiaofujiu";

		public override string UnlockText => string.Empty;

		public override void QueueUnlocks()
		{
			// 不解锁任何内容
		}
	}
}
