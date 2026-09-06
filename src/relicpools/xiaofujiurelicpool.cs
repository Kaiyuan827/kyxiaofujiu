using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;

namespace kyxiaofujiu.relicpools
{
	public sealed class XiaofujiuRelicPool : RelicPoolModel
	{
		public override string EnergyColorName => "xiaofujiu";
		public override Color LabOutlineColor => new Color("785490");

		protected override List<RelicModel> GenerateAllRelics()
		{
			return new List<RelicModel>
			{
				ModelDb.Relic<MyCustomRelic>(),
				ModelDb.Relic<SpireExpert>(),
				ModelDb.Relic<KaJiuRelic>(),
				ModelDb.Relic<XiangluleRelic>(),
				ModelDb.Relic<WeddingRing>(),
				ModelDb.Relic<ProposalRing>(),
				ModelDb.Relic<KaRelic>(),
				ModelDb.Relic<KaHaoLe>(),
				ModelDb.Relic<LuckyLandlordRelic>(),
				ModelDb.Relic<NoSlGaoShou>(),
				ModelDb.Relic<JianTaYaYi>(),
				ModelDb.Relic<EMoQiYueRelic>(),
				ModelDb.Relic<HuaJiaGuanZhong>(),
			};
		}
	}
}
