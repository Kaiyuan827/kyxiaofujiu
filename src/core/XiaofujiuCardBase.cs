using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
	using MegaCrit.Sts2.Core.Nodes.Rooms;
	using kyxiaofujiu.powers;
	using kyxiaofujiu.relicpools;
	using kyxiaofujiu.utils;

namespace kyxiaofujiu.core
{
	public abstract class XiaofujiuCardBase : CardModel
	{
		// ==================== 静态事件 ====================

		/// <summary>
		/// 任意夫黑夫白状态转换时触发（包括黑→白、白→黑）
		/// </summary>
		public static event System.Action<CardModel, FufuState, FufuState>? FufuStateChanged;

		/// <summary>
		/// 夫黑 → 夫白 转换时触发
		/// </summary>
		public static event System.Action<CardModel>? FufuBlackToWhiteConverted;

		/// <summary>
		/// 夫白 → 夫黑 转换时触发
		/// </summary>
		public static event System.Action<CardModel>? FufuWhiteToBlackConverted;

		/// <summary>
		/// 本场战斗各玩家夫态转化次数（按玩家 NetId 隔离；多人下两名玩家互不串扰，双端确定性一致）
		/// 入夫门等卡读取自己玩家的计数；战斗开始由补丁清空。
		/// </summary>
		private static readonly Dictionary<ulong, int> _combatConvertCounts = new Dictionary<ulong, int>();

		/// <summary>记录一次转化（按卡主玩家统计）</summary>
		public static void RecordConvert(Player? owner)
		{
			if (owner == null) return;
			_combatConvertCounts.TryGetValue(owner.NetId, out int c);
			_combatConvertCounts[owner.NetId] = c + 1;
		}

		/// <summary>读取某玩家本场转化次数（多人安全：只统计该玩家自己的转化）</summary>
		public static int GetCombatConvertCount(Player? owner)
		{
			if (owner == null) return 0;
			_combatConvertCounts.TryGetValue(owner.NetId, out int c);
			return c;
		}

		/// <summary>战斗开始清空（由补丁调用）</summary>
		public static void ResetCombatConvertCounts() => _combatConvertCounts.Clear();

		// ==================== 子类可重写 ====================

		/// <summary>
		/// 子类重写此属性设置默认夫黑夫白状态
		/// </summary>
		public virtual FufuState CanonicalFufuState => FufuState.White;  // ✅ 默认夫白

		/// <summary>
		/// 打出这张牌需要花费的工资（0 = 不需要工资）。
		/// 工资是永久跨战斗保留的资源，从【幸运房东】遗物的工资中扣除。
		/// </summary>
		public virtual int SalaryCost => 0;

		/// <summary>
		/// 这张牌是否需要花费工资才能打出
		/// </summary>
		public bool HasSalaryCost => SalaryCost > 0;

		/// <summary>
		/// 当前可用的工资（来自【幸运房东】遗物，跨战斗保留）
		/// </summary>
		public int CurrentSalary
		{
			get
			{
				// canonical 模型（如卡牌大全中的展示卡）没有 Owner，访问 Owner 会抛 AssertMutable，直接返回 0
				if (IsCanonical) return 0;
				var landlord = Owner?.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
				return landlord?.Salary ?? 0;
			}
		}

		/// <summary>
		/// 子类重写此方法提供基础关键词（除夫黑自动添加的保留外）
		/// </summary>
		protected virtual IEnumerable<CardKeyword> GetBaseKeywords() => Enumerable.Empty<CardKeyword>();

		// ==================== 运行时状态 ====================

		private FufuState _fufuState = FufuState.None;

		/// <summary>
		/// 当前夫黑夫白状态
		/// </summary>
		public FufuState FufuState
		{
			get => _fufuState;
			set
			{
				if (_fufuState == value) return;
				var oldState = _fufuState;
				_fufuState = value;
				OnFufuStateChanged(oldState, value);
			}
		}

		public bool IsFufuBlack => FufuState == FufuState.Black;
		public bool IsFufuWhite => FufuState == FufuState.White;
		public bool IsFufuYellow => FufuState == FufuState.Yellow;
		public bool IsFufu => FufuState != FufuState.None;

		// ==================== 构造函数 ====================

		protected XiaofujiuCardBase(
			int canonicalEnergyCost,
			CardType type,
			CardRarity rarity,
			TargetType targetType,
			bool shouldShowInCardLibrary = true)
			: base(canonicalEnergyCost, type, rarity, targetType, shouldShowInCardLibrary)
		{
		}

		// ==================== 关键词管理 ====================

		public override IEnumerable<CardKeyword> CanonicalKeywords
		{
			get
			{
				var baseKeywords = GetBaseKeywords().ToList();
				return baseKeywords.Distinct();
			}
		}

		// ==================== 可打出性 ====================

		protected override bool IsPlayable
		{
			get
			{
				// canonical 模型（卡牌大全展示卡）没有 Owner，跳过依赖 Owner 的判断
				if (!IsCanonical)
				{
					// 夫黑默认不可打出，除非持有者有夫化能力
					if (IsFufuBlack && Owner?.Creature?.HasPower<FuhuaPower>() != true)
						return false;
					// 工资不足时不可打出（需要花费工资的卡牌）
					if (HasSalaryCost && CurrentSalary < SalaryCost)
						return false;
				}
				return base.IsPlayable;
			}
		}

		// ==================== 悬浮提示 ====================

		private static readonly LocString _fufuBlackTitle = new LocString("cards", "FUZU_BLACK.title");
		private static readonly LocString _fufuBlackDesc = new LocString("cards", "FUZU_BLACK.description");
		private static readonly LocString _fufuWhiteTitle = new LocString("cards", "FUZU_WHITE.title");
		private static readonly LocString _fufuWhiteDesc = new LocString("cards", "FUZU_WHITE.description");
		private static readonly LocString _fufuYellowTitle = new LocString("cards", "FUZU_YELLOW.title");
		private static readonly LocString _fufuYellowDesc = new LocString("cards", "FUZU_YELLOW.description");
		private static readonly LocString _salaryTitle = new LocString("cards", "SALARY_MECHANICS_TITLE");
		private static readonly LocString _salaryDesc = new LocString("cards", "SALARY_MECHANICS_DESC");

		/// <summary>
		/// 子类有征婚效果时重写为 true（基类统一添加征婚说明 hover）
		/// </summary>
		public virtual bool HasMarriageEffect => false;

		/// <summary>
		/// 涉及夫黄机制（转化为夫黄/夫黄效果）时重写为 true，hover 额外显示夫黄说明（即使当前非夫黄）
		/// </summary>
		public virtual bool HasYellowEffect => false;

		/// <summary>
		/// 涉及夫黑机制（打出夫黑/手牌夫黑/转化夫黑等）时重写为 true，hover 额外显示夫黑说明（即使当前非夫黑）
		/// </summary>
		public virtual bool HasBlackEffect => false;

		/// <summary>
		/// 涉及转化机制（转化手牌/转化自身/转化计数等）时重写为 true，hover 额外显示转化说明
		/// </summary>
		public virtual bool HasConvertEffect => false;

		/// <summary>
		/// 是否在 hover 显示工资说明（固定工资卡自动 true；动态工资卡如欧金金可 override）
		/// </summary>
		public virtual bool ShowSalaryHover => HasSalaryCost;

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				var tips = new List<IHoverTip>();
				// 夫态状态说明（夫黑/夫白/夫黄）
				if (IsFufuBlack)
				{
					tips.Add(new HoverTip(_fufuBlackTitle, _fufuBlackDesc));
				}
				else if (IsFufuWhite)
				{
					tips.Add(new HoverTip(_fufuWhiteTitle, _fufuWhiteDesc));
				}
				else if (IsFufuYellow)
				{
					tips.Add(new HoverTip(_fufuYellowTitle, _fufuYellowDesc));
				}
				// 涉及夫黄机制但当前非夫黄的卡（如"转化为夫黄"效果），额外显示夫黄说明
				if (HasYellowEffect && !IsFufuYellow)
				{
					tips.Add(new HoverTip(_fufuYellowTitle, _fufuYellowDesc));
				}
				// 涉及夫黑机制但当前非夫黑的卡（如"打出夫黑/手牌夫黑"效果），额外显示夫黑说明
				if (HasBlackEffect && !IsFufuBlack)
				{
					tips.Add(new HoverTip(_fufuBlackTitle, _fufuBlackDesc));
				}
				// 夫黄重放词条：夫黄卡自动附带重放，悬浮补充“重放”说明
				// （StS2 仅对附魔重放(GetEnchantedReplayCount)自动显示，夫黄的 BaseReplayCount 需手动补）
				if (IsFufuYellow)
				{
					int replay = _fufuReplayAdded > 0 ? _fufuReplayAdded : GetYellowReplayCount();
					if (replay > 0)
					{
						tips.Add(HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, new DynamicVar("Times", replay)));
					}
				}
				// 转化机制说明（转化手牌/转化自身/转化计数等，统一由基类添加）
				if (HasConvertEffect)
				{
					tips.AddRange(HoverTipHelper.ConvertTips);
				}
				// 征婚效果说明（统一由基类添加，避免各卡遗漏）
				if (HasMarriageEffect)
				{
					tips.AddRange(HoverTipHelper.MarriageTips);
				}
				// 工资花费说明
				if (ShowSalaryHover)
				{
					tips.Add(new HoverTip(_salaryTitle, _salaryDesc));
				}
				return tips;
			}
		}

		// ==================== 生命周期 ====================

		protected override void AfterCloned()
		{
			base.AfterCloned();
			_fufuState = CanonicalFufuState;
			UpdateFufuKeywords();
			// 延迟刷新，等待卡牌被添加到场景
			Callable.From(delegate
			{
				RefreshCardVisuals();
				RefreshOverlay();
			}).CallDeferred();
		}

		// ==================== 状态变化处理 ====================

		protected virtual void OnFufuStateChanged(FufuState oldState, FufuState newState)
		{
			// 1. 更新关键词（保留/重放）
			UpdateFufuKeywords();

			// 2. 刷新卡牌覆盖层
			RefreshOverlay();

			// 3. 刷新卡牌视觉（如边框、卡面等）
			RefreshCardVisuals();

			// 4. 触发静态事件
			FufuStateChanged?.Invoke(this, oldState, newState);

			if (oldState == FufuState.Black && newState == FufuState.White)
			{
				FufuBlackToWhiteConverted?.Invoke(this);
			}
			else if (oldState == FufuState.White && newState == FufuState.Black)
			{
				FufuWhiteToBlackConverted?.Invoke(this);
			}

			// 5. 通知所有者生物上的能力（非静态事件，随战斗结束自动清理）
			NotifyOwnerPowers(oldState, newState);

			// 6. 转化计数器：按卡主玩家统计本场转化次数（多人下各玩家独立）
			RecordConvert(Owner);
		}

		private void NotifyOwnerPowers(FufuState oldState, FufuState newState)
		{
			if (oldState == FufuState.None || newState == FufuState.None) return;
			if (oldState == newState) return;
			var creature = Owner?.Creature;
			if (creature == null || creature.IsDead) return;

			var power = creature.GetPower<ChuangshifuzhuPower>();
			power?.OnFufuToggled(this, oldState, newState);

			// 一次一个：夫黑→夫白 转化时抽牌（走生物 Power 钩子，战斗结束随 Power 移除自动失效，
			// 避免静态事件订阅跨战斗残留/叠加导致抽牌翻倍超手牌上限）
			if (oldState == FufuState.Black && newState == FufuState.White)
			{
				creature.GetPower<YiciyigePower>()?.OnFufuBlackToWhiteConverted(this);
			}
		}

		// ==================== 关键词更新 ====================

		// 记录"夫态是否主动添加了 Retain"：夫白时只移除夫态添加的保留，
		// 不影响卡牌自带保留（CanonicalKeywords）和附魔获得的保留（如 Steady 附魔）
		private bool _fufuRetainAdded;
		// 记录"夫态添加的重放数"：夫黄重放增量叠加，不覆盖卡牌原有重放（如附魔重放）
		private int _fufuReplayAdded;

		private void UpdateFufuKeywords()
		{
			// 夫黑/夫黄获得保留（若卡牌本身/附魔已有保留则不重复添加，避免标记误判）
			if (IsFufuBlack || IsFufuYellow)
			{
				if (!Keywords.Contains(CardKeyword.Retain))
				{
					AddKeyword(CardKeyword.Retain);
					_fufuRetainAdded = true;
				}
			}
			else
			{
				// 夫白：只移除"由夫态添加"的保留，卡牌自带/附魔保留保留
				if (_fufuRetainAdded)
				{
					RemoveKeyword(CardKeyword.Retain);
					_fufuRetainAdded = false;
				}
			}

			// 夫黄重放：先移除之前夫态添加的重放，再在当前基础上增量叠加，
			// 不覆盖卡牌原有重放（如"未掘宝石"附魔基于 BaseReplayCount 叠加）
			if (_fufuReplayAdded > 0)
			{
				BaseReplayCount = Math.Max(0, BaseReplayCount - _fufuReplayAdded);
				_fufuReplayAdded = 0;
			}
			if (IsFufuYellow)
			{
				int yellowReplay = GetYellowReplayCount();
				BaseReplayCount += yellowReplay;
				_fufuReplayAdded = yellowReplay;
			}
		}

		/// <summary>
		/// 夫黄的基础重放次数：默认1，每张【黄皮夫才对】能力额外+1（可叠加）
		/// </summary>
		private int GetYellowReplayCount()
		{
			int count = 1;
			// canonical 模型（卡牌大全展示卡）无 Owner，只算基础重放
			if (!IsCanonical && Owner?.Creature != null)
			{
				var power = Owner.Creature.GetPower<HuangpifucaideiPower>();
				if (power != null)
				{
					count += power.Amount;
				}
			}
			return count;
		}

		/// <summary>
		/// 【黄皮夫才对】能力施加/移除后，刷新玩家所有夫黄卡的重放次数
		/// </summary>
		public static void RefreshReplayCounts(Player? player)
		{
			if (player?.PlayerCombatState == null) return;
			foreach (var card in player.PlayerCombatState.AllCards)
			{
				if (card is XiaofujiuCardBase fc && fc.IsFufuYellow)
				{
					fc.UpdateFufuKeywords();  // 重算夫黄重放（增量叠加）
				}
			}
		}

		// ==================== 覆盖层刷新 ====================

		private void RefreshOverlay()
		{
			var cardNode = NCard.FindOnTable(this);
			if (cardNode == null) return;

			// 清除旧覆盖层并添加新覆盖层
			FufuOverlayHelper.Clear(cardNode);
			if (IsFufu)
			{
				FufuOverlayHelper.Add(cardNode, this);
			}
		}

		// ==================== 卡牌视觉刷新 ====================

		private void RefreshCardVisuals()
		{
			var cardNode = NCard.FindOnTable(this);
			if (cardNode == null) return;

			cardNode.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
			cardNode.QueueRedraw();
		}

		// ==================== 便捷方法 ====================

		// 夫黄不可再被转化
		public void Seal() { if (FufuState != FufuState.Yellow) FufuState = FufuState.Black; }
		public void Unseal() { if (FufuState != FufuState.Yellow) FufuState = FufuState.White; }
		public void ToggleFufu()
		{
			if (FufuState == FufuState.Yellow) return;
			FufuState = IsFufuBlack ? FufuState.White : FufuState.Black;
		}

		/// <summary>将夫黑/夫白转化为夫黄（夫黄获得保留与重放1，不可再被转化）</summary>
		public void ConvertToYellow()
		{
			if (IsFufuBlack || IsFufuWhite)
			{
				FufuState = FufuState.Yellow;
			}
		}
	}
}
