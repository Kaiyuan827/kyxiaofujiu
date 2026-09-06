using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Potions;
using kyxiaofujiu.potionpools;

namespace kyxiaofujiu.PotionPools
{
	public sealed class XiaofujiuPotionPool : PotionPoolModel
	{
		public override string EnergyColorName => "xiaofujiu";
		public override Color LabOutlineColor => new Color("#C546EC");

		protected override List<PotionModel> GenerateAllPotions()
		{
			return new List<PotionModel>
			{
				// 专属药水
				ModelDb.Potion<ZhenghunPotion>(),
				ModelDb.Potion<SalaryPotion>(),
				ModelDb.Potion<FufuYellowPotion>(),
			};
		}
	}
}
