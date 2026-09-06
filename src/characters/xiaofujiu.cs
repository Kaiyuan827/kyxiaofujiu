using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Cards;
using kyxiaofujiu.cardpools;
using kyxiaofujiu.PotionPools;
using kyxiaofujiu.relicpools;

namespace kyxiaofujiu.characters
{
	public sealed class Xiaofujiu : CharacterModel
	{
		public const string EnergyColorName = "xiaofujiu";

		public override CharacterGender Gender => CharacterGender.Feminine;

		protected override CharacterModel? UnlocksAfterRunAs => null;

		// 改为黄色（金色）
		public override Color NameColor => new Color("#FFD700");
		

		public override int StartingHp => 72;
		public override int StartingGold => 99;

		public override CardPoolModel CardPool => ModelDb.CardPool<XiaofujiuCardPool>();
		public override PotionPoolModel PotionPool => ModelDb.PotionPool<XiaofujiuPotionPool>();
		public override RelicPoolModel RelicPool => ModelDb.RelicPool<XiaofujiuRelicPool>();

		public override IReadOnlyList<CardModel> StartingDeck => new List<CardModel>
		{
			ModelDb.Card<Strikexiaofujiu>(),
			ModelDb.Card<Strikexiaofujiu>(),
			ModelDb.Card<Strikexiaofujiu>(),
			ModelDb.Card<Strikexiaofujiu>(),

			ModelDb.Card<Defendxiaofujiu>(),
			ModelDb.Card<Defendxiaofujiu>(),
			ModelDb.Card<Defendxiaofujiu>(),
			ModelDb.Card<Defendxiaofujiu>(),
			// 初始特殊牌
			ModelDb.Card<Dagongren>(),        
			ModelDb.Card<Heizhuanbaizhuanhei>(), 
		};

		public override IReadOnlyList<RelicModel> StartingRelics => new List<RelicModel>
		{
			// ModelDb.Relic<MyCustomRelic>(), // TODO: 暂时注释"尖塔高手"
			ModelDb.Relic<KaJiuRelic>(),
			ModelDb.Relic<LuckyLandlordRelic>(), // 幸运房东（工资系统，不覆盖原初始遗物）
		};

		public override float AttackAnimDelay => 0.15f;
		public override float CastAnimDelay => 0.25f;

		// 能量数字描边（已是黄色调，保留）
		public override Color EnergyLabelOutlineColor => new Color("#9A8E00FF");

		// 其他所有颜色改为黄色
		public override Color DialogueColor => new Color("#FFD700");
		public override Color MapDrawingColor => new Color("#FFD700");
		public override Color RemoteTargetingLineColor => new Color("#FFD700");
		public override Color RemoteTargetingLineOutline => new Color("#FFD700");

		public override List<string> GetArchitectAttackVfx() => new List<string>();
		

	}
}
