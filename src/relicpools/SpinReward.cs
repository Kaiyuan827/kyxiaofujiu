using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace kyxiaofujiu.relicpools
{
	/// <summary>
	/// 旋转一次 — 幸运房东收租后出现在战斗战利品页面的奖励，
	/// 玩家点击后执行一次房东抽奖（GrantReward）。
	/// RewardType.None 作为标记；SL 恢复由补丁 Reward.FromSerializable 兜底。
	/// </summary>
	public sealed class SpinReward : Reward
	{
		private static string RewardIcon => ImageHelper.GetImagePath("relics/lucky_landlord_relic.png");

		private readonly LuckyLandlordRelic _relic;

		protected override RewardType RewardType => RewardType.None;

		public override int RewardsSetIndex => 100;  // 排在最后

		protected override string IconPath => RewardIcon;

		public override LocString Description => new LocString("relics", "LUCKY_LANDLORD_RELIC.spinReward");

		public override bool IsPopulated => true;

		public SpinReward(Player player, LuckyLandlordRelic relic)
			: base(player)
		{
			_relic = relic;
		}

		public override void Populate()
		{
		}

		protected override async Task<bool> OnSelect()
		{
			if (_relic == null) return false;
			await _relic.GrantReward(Player);
			return true;
		}

		public override void MarkContentAsSeen()
		{
		}

		public override SerializableReward ToSerializable()
		{
			return new SerializableReward { RewardType = RewardType.None };
		}
	}
}
