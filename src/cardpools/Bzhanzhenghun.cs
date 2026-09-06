using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;


using MegaCrit.Sts2.Core.HoverTips; 
using kyxiaofujiu.utils; 

namespace kyxiaofujiu.cardpools


{
	public sealed class Bzhanzhenghun : XiaofujiuCardBase
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new IntVar("MarryCount", 3m)  // 征婚3次，升级4次
		};
		
		// 征婚效果：基类统一添加征婚 + 夫态 hover
		public override bool HasMarriageEffect => true;


		public Bzhanzhenghun()
			: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var marryCount = (IntVar)DynamicVars["MarryCount"];
			int count = (int)marryCount.BaseValue;
			await ZhengHunCmd.Marry(choiceContext, base.Owner, count);
		}

		protected override void OnUpgrade()
		{
			var marryCount = (IntVar)DynamicVars["MarryCount"];
			marryCount.UpgradeValueBy(1m);
		}
	}
}
