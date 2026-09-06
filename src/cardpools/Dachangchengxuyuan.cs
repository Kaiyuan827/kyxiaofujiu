using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.commands;
using MegaCrit.Sts2.Core.HoverTips; 
using kyxiaofujiu.utils; 
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Dachangchengxuyuan : XiaofujiuCardBase
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new DamageVar(6m, ValueProp.Move)
		};
		
		// 征婚效果：基类统一添加征婚 + 夫态 hover
		public override bool HasMarriageEffect => true;

		public Dachangchengxuyuan()
			: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}
		


		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var target = cardPlay.Target;
			if (target == null) return;

			// 造成6点伤害
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			// 征婚2（平衡性调整）
			await ZhengHunCmd.Marry(choiceContext, Owner, 2);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
