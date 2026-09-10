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

		// 敌方行动顺序 = Slots 下标顺序（SortEnemiesBySlotName 按此重排）。
		// 把 boss 放到最后：让敌方蛇花（snake1..5）先攻、Boss 最后攻，
		// 避免"易伤回合 Boss 先手挂易伤 -> 蛇花后手伤害被放大成 12*3"造成意图(回合初已显示 8*3)不一致。
		public override IReadOnlyList<string> Slots => new[] { "snake1", "snake2", "snake3", "snake4", "snake5", "boss" };

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
