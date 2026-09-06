using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using kyxiaofujiu.monsters;

namespace kyxiaofujiu.monsters
{
	/// <summary>
	/// 白区 Boss 遭遇
	/// </summary>
	public sealed class BaoQuBoss : EncounterModel
	{
		public override RoomType RoomType => RoomType.Boss;

		// 使用遭遇场景中的 Marker2D 槽位（boss 中央，snake1~5 分布四周）
		public override bool HasScene => true;

		public override IReadOnlyList<string> Slots => new[] { "boss", "snake1", "snake2", "snake3", "snake4", "snake5" };

		public override string BossNodePath => "res://images/map/placeholder/" + base.Id.Entry.ToLowerInvariant() + "_icon";

		// 自定义战斗音乐标记（由补丁12拦截，播放 mod 的 mp3）
		public override string CustomBgm => "custom:yao_jiu_yao_jia_xiao_fu_jiu";

		// 背景改为代码动态添加（CustomBackgroundManager），此处保持游戏默认背景打底
		// protected override bool HasCustomBackground => true;

		public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
		{
			ModelDb.Monster<BaoQu>()
		};

		protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
		{
			return new List<(MonsterModel, string?)>
			{
				(ModelDb.Monster<BaoQu>().ToMutable(), "boss")
			};
		}
	}
}
