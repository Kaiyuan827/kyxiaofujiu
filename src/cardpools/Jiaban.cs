using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using kyxiaofujiu.commands;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Jiaban : XiaofujiuCardBase
	{
		public override bool ShowSalaryHover => true;
		public override bool CanBeGeneratedInCombat => false;

		protected override IEnumerable<DynamicVar> CanonicalVars => new[]
		{
			new IntVar("Salary", 15m)
		};

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				var baseTips = base.ExtraHoverTips;
				if (baseTips != null) tips.AddRange(baseTips);
				tips.Add(HoverTipFactory.FromPower<WeakPower>());
				return tips;
			}
		}

		public Jiaban()
			: base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
		{
		}

		public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
		{
			CardKeyword.Exhaust
		};

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Log.Info("=== Jiaban.OnPlay 被调用 ===");
			int salary = (int)DynamicVars["Salary"].BaseValue;
			Log.Info($"=== 获得工资: {salary} ===");
			SalaryCmd.Gain(Owner, salary);

			Log.Info("=== 施加虚弱 ===");
			await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
			var weak = Owner.Creature.GetPower<WeakPower>();
			if (weak != null)
			{
				weak.SkipNextDurationTick = false;
				Log.Info("=== 虚弱 SkipNextDurationTick = false ===");
			}
			Log.Info("=== Jiaban.OnPlay 完成 ===");
		}

		protected override void OnUpgrade()
		{
			Log.Info("=== Jiaban.OnUpgrade 被调用 ===");
			DynamicVars["Salary"].UpgradeValueBy(5m);
			Log.Info($"=== 工资升级后: {(int)DynamicVars["Salary"].BaseValue} ===");
		}

		public static async Task<CardModel?> CreateInHand(Player owner, ICombatState combatState)
{
	Log.Info("=== Jiaban.CreateInHand 开始 ===");
	
	try
	{
		// ✅ 使用 combatState.CreateCard 正确创建与 CombatState 关联的卡牌
		var card = combatState.CreateCard<Jiaban>(owner);
		
		await CardPileCmd.AddGeneratedCardsToCombat(new[] { card }, PileType.Hand, owner);
		Log.Info("=== 卡牌已加入手牌 ===");
		return card;
	}
	catch (System.Exception ex)
	{
		Log.Error($"=== Jiaban.CreateInHand 异常: {ex.Message} ===");
		Log.Error(ex.StackTrace);
		return null;
	}
}
	}
}
