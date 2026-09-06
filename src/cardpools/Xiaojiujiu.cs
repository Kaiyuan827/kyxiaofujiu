using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using kyxiaofujiu.core;


namespace kyxiaofujiu.cardpools
{
	public sealed class Xiaojiujiu : XiaofujiuCardBase
	{
		private const int BaseDamage = 3;
		private const int BaseBlock = 2;

		private int _currentDamage = BaseDamage;
		private int _currentBlock = BaseBlock;
		private int _increasedDamage;
		private int _increasedBlock;

		public override bool GainsBlock => true;

		[SavedProperty]
		public int CurrentDamage
		{
			get => _currentDamage;
			set
			{
				AssertMutable();
				_currentDamage = value;
				base.DynamicVars.Damage.BaseValue = _currentDamage;
			}
		}

		[SavedProperty]
		public int CurrentBlock
		{
			get => _currentBlock;
			set
			{
				AssertMutable();
				_currentBlock = value;
				base.DynamicVars.Block.BaseValue = _currentBlock;
			}
		}

		[SavedProperty]
		public int IncreasedDamage
		{
			get => _increasedDamage;
			set
			{
				AssertMutable();
				_increasedDamage = value;
			}
		}

		[SavedProperty]
		public int IncreasedBlock
		{
			get => _increasedBlock;
			set
			{
				AssertMutable();
				_increasedBlock = value;
			}
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
		{
			new DamageVar(CurrentDamage, ValueProp.Move),
			new BlockVar(CurrentBlock, ValueProp.Move),
			new IntVar("DamageIncrease", 2m),
			new IntVar("BlockIncrease", 1m)
		};

		public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
		{
			CardKeyword.Exhaust
		};

		public Xiaojiujiu()
			: base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)  // ✅ 改为稀有
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			var target = cardPlay.Target;
			if (target == null) return;

			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
				.FromCard(this, cardPlay)
				.Targeting(target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);

			await CreatureCmd.GainBlock(Owner.Creature, base.DynamicVars.Block, cardPlay);

			int damageIncrease = base.DynamicVars["DamageIncrease"].IntValue;
			int blockIncrease = base.DynamicVars["BlockIncrease"].IntValue;

			BuffFromPlay(damageIncrease, blockIncrease);
			(base.DeckVersion as Xiaojiujiu)?.BuffFromPlay(damageIncrease, blockIncrease);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars["DamageIncrease"].UpgradeValueBy(1m);  // 2 → 3
			base.DynamicVars["BlockIncrease"].UpgradeValueBy(1m);   // 1 → 2
		}

		protected override void AfterDowngraded()
		{
			UpdateValues();
		}

		private void BuffFromPlay(int extraDamage, int extraBlock)
		{
			IncreasedDamage += extraDamage;
			IncreasedBlock += extraBlock;
			UpdateValues();
		}

		private void UpdateValues()
		{
			CurrentDamage = BaseDamage + IncreasedDamage;
			CurrentBlock = BaseBlock + IncreasedBlock;
		}
	}
}
