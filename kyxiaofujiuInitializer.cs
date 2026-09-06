using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using kyxiaofujiu.cardpools;
using kyxiaofujiu.relicpools;
using kyxiaofujiu.characters;
using kyxiaofujiu.events;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;

using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Timeline;
using kyxiaofujiu.epochs;

	using MegaCrit.Sts2.Core.Saves.Managers;
	using MegaCrit.Sts2.Core.Multiplayer.Serialization;

	using kyxiaofujiu.powers;
	using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;

using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Nodes.CommonUi;  // 
using MegaCrit.Sts2.Core.Commands;        // ✅ Cmd 在此

using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Combat;

	using MegaCrit.Sts2.Core.Saves.Runs;  // ← 需要添加这个 using
	using MegaCrit.Sts2.Core.Rewards;     // Reward / RewardType（SpinReward SL 恢复）

	using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

	using kyxiaofujiu.core;

	using MegaCrit.Sts2.Core.Nodes.Cards;

using kyxiaofujiu.utils;
using MegaCrit.Sts2.Core.Nodes.Audio;

using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

using kyxiaofujiu.commands;

	using kyxiaofujiu.monsters;






namespace kyxiaofujiu
{
	[ModInitializer(nameof(Initialize))]
	public static class kyxiaofujiuInitializer
	{
		public static void Initialize()
		{
			try
			{
				// 1. 建立Godot脚本映射，让场景能找到CustomCreatureVisuals等自定义脚本类
				Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
				//SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(Xiaojiujiu));


				// 2. 读取兼容模式配置，选择性应用Harmony补丁（防止与其他mod冲突）
				//    兼容模式开启时只应用人物选择/角色基础补丁，其余机制补丁跳过（重启生效）
				XiaofujiuCompat.Load();
				var harmony = new Harmony("kyxiaofujiu.xiaofujiu");
				XiaofujiuCompat.ApplyPatches(harmony);

				Log.Info("kyxiaofujiu 加载成功" + (XiaofujiuCompat.CompatibilityMode ? "（兼容模式：仅人物选择相关补丁）" : ""));
			}
			
			
			catch (Exception e)
			{
				Log.Error($"kyxiaofujiu 加载失败: {e.Message}");
				Log.Error(e.StackTrace);
			}
		}
	}

	// ========== Harmony 补丁 ==========

	[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
	public static class ModelDbAllCharactersPatch
	{
		static void Postfix(ref IEnumerable<CharacterModel> __result)
		{
			try
			{
				var xiaofujiu = ModelDb.Character<Xiaofujiu>();
				if (xiaofujiu != null)
				{
					__result = __result.Append(xiaofujiu).Distinct();
				}
				else
				{
					Log.Error("ModelDb.Character<Xiaofujiu>() 返回 null");
				}
			}
			catch (Exception e)
			{
				Log.Error($"AllCharacters 补丁异常: {e.Message}");
			}
		}
	}

	[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
	public static class ModelDbAllCardPoolsPatch
	{
		static void Postfix(ref IEnumerable<CardPoolModel> __result)
		{
			try
			{
				var pool = ModelDb.CardPool<XiaofujiuCardPool>();
				if (pool != null)
				{
					var list = __result.ToList();
					// 不复用角色卡池段已有的池，避免重复
					if (!list.Contains(pool))
					{
						// 找到最后一个角色卡池之后的位置，插在共用池（无色/诅咒/事件/任务/状态/衍生牌）之前，
						// 使晓夫九卡池排在其它角色卡池之后、衍生牌之前。
						var characterPools = ModelDb.AllCharacterCardPools.ToHashSet();
						int insertIndex = 0;
						for (int i = 0; i < list.Count; i++)
						{
							if (characterPools.Contains(list[i]))
							{
								insertIndex = i + 1;
							}
						}
						list.Insert(insertIndex, pool);
					}
					__result = list;
				}
				else
				{
					Log.Error("ModelDb.CardPool<XiaofujiuCardPool>() 返回 null");
				}
			}
			catch (Exception e)
			{
				Log.Error($"AllCardPools 补丁异常: {e.Message}");
			}
		}
	}

	[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllRelicPools), MethodType.Getter)]
	public static class ModelDbAllRelicPoolsPatch
	{
		static void Postfix(ref IEnumerable<RelicPoolModel> __result)
		{
			try
			{
				var pool = ModelDb.RelicPool<XiaofujiuRelicPool>();
				if (pool != null)
				{
					__result = __result.Append(pool).Distinct();
				}
				else
				{
					Log.Error("ModelDb.RelicPool<XiaofujiuRelicPool>() 返回 null");
				}
			}
			catch (Exception e)
			{
				Log.Error($"AllRelicPools 补丁异常: {e.Message}");
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.EnergyCounterPath), MethodType.Getter)]
	public static class CharacterModel_EnergyCounterPath_Patch
	{
		static void Postfix(CharacterModel __instance, ref string __result)
		{
			if (__instance is Xiaofujiu)
			{
				__result = "res://scenes/combat/energy_counters/xiaofujiu_energy_counter.tscn";
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.TrailPath), MethodType.Getter)]
	public static class CharacterModel_TrailPath_Patch
	{
		static void Postfix(CharacterModel __instance, ref string __result)
		{
			if (__instance is Xiaofujiu)
			{
				__result = "res://scenes/vfx/card_trail_ironclad.tscn";
			}
		}
	}
	


	
	[HarmonyPatch(typeof(TokenCardPool), "GenerateAllCards")]
public static class TokenCardPoolPatch
{
	static void Postfix(ref CardModel[] __result)
	{
		try
		{
			var list = __result.ToList();
			list.Add(ModelDb.Card<Jiaban>());              ///加班的补丁
			__result = list.ToArray();
			Log.Info("TokenCardPool 补丁成功: 已添加 Jiaban");
		}
		catch (Exception e)
		{
			Log.Error($"TokenCardPool 补丁失败: {e.Message}");
		}
	}
}

// 1. EpochModel.Get - 返回自定义纪元实例
[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.Get), new Type[] { typeof(string) })]
public static class EpochModel_Get_Patch
{
	static bool Prefix(string id, ref EpochModel __result)
	{
		if (id == "XIAOFUJIU2_EPOCH")
		{
			__result = new Xiaofujiu2Epoch();
			return false;
		}
		if (id == "XIAOFUJIU3_EPOCH")
		{
			__result = new Xiaofujiu3Epoch();
			return false;
		}
		if (id == "XIAOFUJIU4_EPOCH")
		{
			__result = new Xiaofujiu4Epoch();
			return false;
		}
		return true;
	}
}

// 2. 跳过 Check 方法
[HarmonyPatch("MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager", "CheckFifteenElitesDefeatedEpoch")]
public static class ProgressSaveManager_CheckFifteenElitesDefeatedEpoch_Patch
{
	static bool Prefix(Player localPlayer)
	{
		if (localPlayer.Character is Xiaofujiu) return false;
		return true;
	}
}

// 3. 跳过 CheckFifteenBossesDefeatedEpoch（如果需要）
[HarmonyPatch("MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager", "CheckFifteenBossesDefeatedEpoch")]
public static class ProgressSaveManager_CheckFifteenBossesDefeatedEpoch_Patch
{
	static bool Prefix(Player localPlayer)
	{
		if (localPlayer.Character is Xiaofujiu) return false;
		return true;
	}
}

	[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.AllEpochIds), MethodType.Getter)]
public static class EpochModel_AllEpochIds_Patch
{
	static void Postfix(ref IReadOnlyList<string> __result)
	{
		var list = __result.ToList();
		
		// 添加晓夫九的纪元ID
		if (!list.Contains("XIAOFUJIU2_EPOCH"))
			list.Add("XIAOFUJIU2_EPOCH");
		if (!list.Contains("XIAOFUJIU3_EPOCH"))
			list.Add("XIAOFUJIU3_EPOCH");
		if (!list.Contains("XIAOFUJIU4_EPOCH"))
			list.Add("XIAOFUJIU4_EPOCH");
		// 如果还需要更多层，继续添加
		
		__result = list;
	}
}

[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
public static class ArchaicToothTranscendenceUpgradesPatch
{
	static void Postfix(ref Dictionary<ModelId, CardModel> __result)
	{
		var dagongren = ModelDb.Card<Dagongren>();
		var tianxuan = ModelDb.Card<Tianxuandagongren>();           ///欧罗巴斯的遗物
 
		if (!__result.ContainsKey(dagongren.Id))
		{
			__result[dagongren.Id] = tianxuan;
		}
	}
}

// 欧罗巴斯之触：晓夫九选择"欧罗巴斯之触"后，将起始遗物"卡九"替换为"我香炉了"
[HarmonyPatch(typeof(TouchOfOrobas), "RefinementUpgrades", MethodType.Getter)]
public static class TouchOfOrobas_RefinementUpgrades_Patch
{
	static void Postfix(ref Dictionary<ModelId, RelicModel> __result)
	{
		try
		{
			var kaJiu = ModelDb.Relic<KaJiuRelic>();
			var xianglu = ModelDb.Relic<XiangluleRelic>();
			if (kaJiu != null && xianglu != null)
			{
				__result[kaJiu.Id] = xianglu;
				Log.Info("[TouchOfOrobas] 已添加 卡九 → 我香炉了");
			}
		}
		catch (Exception e)
		{
			Log.Error($"[TouchOfOrobas_RefinementUpgrades_Patch] 失败: {e.Message}");
		}
	}
}

// ✅ 修复 PersonalHivePower：蛇花小姐攻击时崩溃
[HarmonyPatch("MegaCrit.Sts2.Core.Models.Powers.PersonalHivePower", "AfterDamageReceived")]
public static class PersonalHivePowerAfterDamageReceivedPatch
{
	static void Prefix(ref Creature? dealer)
	{
		// 如果伤害来源是宠物（蛇花小姐/奥斯提），替换为玩家本人
		if (dealer != null && dealer.PetOwner != null)
		{
			dealer = dealer.PetOwner.Creature;
		}
	}
}

// 晓夫九 AnimatedSprite2D 战斗动画：把 Spine 触发器映射到自定义动画播放
// （攻击 Attack→attack、技能 Cast→skill、能力 PowerUp→power、受击 Hit→hurt、死亡 Dead→death）
[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
public static class NCreature_SetAnimationTrigger_Patch
{
	static void Postfix(NCreature __instance, string trigger)
	{
		try
		{
			if (__instance.Entity == null || !__instance.Entity.IsPlayer) return;
			if (__instance.Entity.Player?.Character is not Xiaofujiu) return;
			if (__instance.Visuals is not CustomCreatureVisuals visuals) return;

			string anim = trigger switch
			{
				"Attack" => "attack",
				"Cast" => "skill",
				"PowerUp" => "power",
				"Hit" => "hurt",
				"Dead" => "death",
				"Idle" => "idle",
				_ => "",
			};
			if (anim.Length > 0)
			{
				visuals.PlayAction(anim);
			}
		}
		catch (Exception e)
		{
			Log.Error($"[NCreature_SetAnimationTrigger_Patch] 失败: {e.Message}");
		}
	}
}

/// <summary>
/// 晓夫九死亡动画：游戏 StartDeathAnim 里的 SetAnimationTrigger("Dead") 只在有 Spine
/// 动画时触发（_spineAnimator != null），晓夫九用 AnimatedSprite2D 无 Spine → 收不到，
/// 死亡时没有死亡动画。此补丁在 StartDeathAnim 时直接对晓夫九播放 death 动画。
/// （玩家死亡 shouldRemove=false，角色不立即移除，death 动画可播完保持最后一帧）
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
public static class NCreature_StartDeathAnim_Patch
{
	static void Prefix(NCreature __instance)
	{
		try
		{
			if (__instance.Entity == null || !__instance.Entity.IsPlayer) return;
			if (__instance.Entity.Player?.Character is not Xiaofujiu) return;
			if (__instance.Visuals is not CustomCreatureVisuals visuals) return;
			visuals.PlayAction("death");
		}
		catch (Exception e)
		{
			Log.Error($"[NCreature_StartDeathAnim_Patch] 失败: {e.Message}");
		}
	}
}

[HarmonyPatch(typeof(ModelIdSerializationCache), nameof(ModelIdSerializationCache.Init))]
public static class ModelIdSerializationCache_Init_Patch
{
	static void Postfix()
	{
		var cacheType = typeof(ModelIdSerializationCache);

		// ✅ 新版本字段名是 _epochNameToNetIdMap
		var epochToNetIdField = cacheType.GetField("_epochNameToNetIdMap",
			BindingFlags.NonPublic | BindingFlags.Static);

		if (epochToNetIdField == null)
		{
			Log.Error("无法找到 _epochNameToNetIdMap 字段");
			return;
		}

		var epochToNetId = epochToNetIdField.GetValue(null) as Dictionary<string, int>;
		if (epochToNetId == null)
		{
			Log.Error("_epochNameToNetIdMap 不是 Dictionary<string, int> 类型");
			return;
		}

		// ✅ 也获取反向映射表 _netIdToEpochNameMap
		var netIdToEpochField = cacheType.GetField("_netIdToEpochNameMap",
			BindingFlags.NonPublic | BindingFlags.Static);

		if (netIdToEpochField == null)
		{
			Log.Error("无法找到 _netIdToEpochNameMap 字段");
			return;
		}

		var netIdToEpoch = netIdToEpochField.GetValue(null) as List<string>;
		if (netIdToEpoch == null)
		{
			Log.Error("_netIdToEpochNameMap 不是 List<string> 类型");
			return;
		}

		string[] customEpochs = { "XIAOFUJIU2_EPOCH", "XIAOFUJIU3_EPOCH", "XIAOFUJIU4_EPOCH" };

		foreach (var epochId in customEpochs)
		{
			if (!epochToNetId.ContainsKey(epochId))
			{
				int newId = netIdToEpoch.Count;
				epochToNetId[epochId] = newId;
				netIdToEpoch.Add(epochId);
				Log.Info($"已注入 {epochId}, NetId = {newId}");
			}
		}
	}
}

// ========== 夫黑夫白覆盖层补丁（最终版） ==========

/// <summary>
/// 补丁1：卡牌模型改变时清除覆盖层（解决对象池重用问题）
/// </summary>
[HarmonyPatch(typeof(NCard), "set_Model")]
public static class NCard_SetModel_Patch
{
	static void Prefix(NCard __instance)
	{
		FufuOverlayHelper.Clear(__instance);
		SalaryOverlayHelper.Clear(__instance);
	}
}

/// <summary>
/// 补丁2：卡牌首次显示时添加覆盖层
/// </summary>
[HarmonyPatch(typeof(NCard), "_Ready")]
public static class NCard_Ready_Patch
{
	static void Postfix(NCard __instance)
	{
		if (__instance.Model is XiaofujiuCardBase baseCard)
		{
			if (baseCard.IsFufu)
			{
				FufuOverlayHelper.Add(__instance, baseCard);
			}
			if (baseCard.HasSalaryCost)
			{
				SalaryOverlayHelper.Add(__instance, baseCard);
			}
		}
	}
}

/// <summary>
/// 补丁3：卡牌被回收时清除覆盖层
/// </summary>
[HarmonyPatch(typeof(NCard), "OnReturnedFromPool")]
public static class NCard_OnReturnedFromPool_Patch
{
	static void Postfix(NCard __instance)
	{
		FufuOverlayHelper.Clear(__instance);
		SalaryOverlayHelper.Clear(__instance);
	}
}

/// <summary>
/// 补丁4：卡牌视觉更新时刷新覆盖层（处理手牌中布局变化）
/// </summary>
[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class NCard_UpdateVisuals_Patch
{
	static void Postfix(NCard __instance)
	{
		if (__instance.Model is XiaofujiuCardBase baseCard)
		{
			if (baseCard.IsFufu)
			{
				FufuOverlayHelper.Add(__instance, baseCard);
			}
			if (baseCard.HasSalaryCost)
			{
				SalaryOverlayHelper.Add(__instance, baseCard);
				SalaryOverlayHelper.UpdateAffordability(__instance, baseCard);
			}
		}
	}
}

/// <summary>
/// 补丁6：打出需要工资的卡牌时，从【幸运房东】工资中扣除对应花费
/// （SpendResources 在手动打出时调用；自动打出不扣工资，与游戏内自动打出不扣能量一致）
/// 免费打出不扣工资：
///   ① 所有 star 免费 —— 药水 SetToFreeThisTurn（SetStarCostThisTurn(0)）与艳丽围巾（TryModifyStarCost 全局 hook）
///      都会让 GetStarCostWithModifiers() == 0（工资卡无 star 时正常返回 -1，被免费后返回 0）
///   ② 被夫黑状态打出 —— 夫黑说话强制打出 / 夫化能力免费打出夫黑（IsFufuBlack）
/// </summary>
[HarmonyPatch(typeof(CardModel), "SpendResources")]
public static class CardModel_SpendResources_Patch
{
	static void Postfix(CardModel __instance)
	{
		if (__instance is XiaofujiuCardBase baseCard && baseCard.HasSalaryCost)
		{
			// ① 所有 star 免费（本次打出 star 费用被豁免为 0）→ 工资免费
			if (__instance.GetStarCostWithModifiers() == 0) return;
			// ② 被夫黑状态打出 → 工资免费
			if (baseCard.IsFufuBlack) return;

			var landlord = __instance.Owner?.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
			if (landlord != null)
			{
				landlord.Salary = Math.Max(0, landlord.Salary - baseCard.SalaryCost);
			}
		}
	}
}

/// <summary>
/// 补丁5：卡牌大全中添加晓夫九卡池筛选按钮
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
public static class NCardLibrary_Ready_Patch
{
	static void Postfix(NCardLibrary __instance)
	{
		try
		{
			// 通过 Traverse 访问私有字段
			var trav = Traverse.Create(__instance);

			var poolFilters = trav.Field<Dictionary<NCardPoolFilter, Func<CardModel, bool>>>("_poolFilters").Value;
			var cardPoolFilters = trav.Field<Dictionary<CharacterModel, NCardPoolFilter>>("_cardPoolFilters").Value;
			var miscFilter = trav.Field<NCardPoolFilter>("_miscPoolFilter").Value;

			if (poolFilters == null || miscFilter == null) return;

			// 克隆 misc 筛选按钮作为晓夫九按钮
			var xiaofujiuFilter = (NCardPoolFilter)miscFilter.Duplicate();
			xiaofujiuFilter.Name = "XiaofujiuPool";
			// 角色筛选按钮不设置悬浮提示（与原版角色一致），避免显示 characters.XIAOFUJIU.cardPoolTitle
			xiaofujiuFilter.Loc = null;

			// 替换图标为晓夫九图标
			TrySetIcon(xiaofujiuFilter);

			// 添加到父容器
			var parent = miscFilter.GetParent();
			if (parent != null)
			{
				parent.AddChild(xiaofujiuFilter);
			}

			// 注册到 _poolFilters
			poolFilters.Add(xiaofujiuFilter, (CardModel c) => c.Pool is XiaofujiuCardPool);

			// 注册到 _cardPoolFilters（关联晓夫九角色）
			var xiaofujiuChar = ModelDb.Character<Xiaofujiu>();
			if (xiaofujiuChar != null && cardPoolFilters != null)
			{
				cardPoolFilters.Add(xiaofujiuChar, xiaofujiuFilter);
			}

			// 连接 Toggled 信号
			var updateMethod = typeof(NCardLibrary).GetMethod("UpdateCardPoolFilter",
				BindingFlags.NonPublic | BindingFlags.Instance);
			if (updateMethod != null)
			{
				var callable = Callable.From((Action<NCardPoolFilter>)Delegate.CreateDelegate(
					typeof(Action<NCardPoolFilter>), __instance, updateMethod));
				xiaofujiuFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
			}

			xiaofujiuFilter.Connect("focus_entered", Callable.From(() =>
			{
				var lastHovered = trav.Field<Control>("_lastHoveredControl");
				lastHovered.Value = xiaofujiuFilter;
			}));

			Log.Info("[NCardLibrary_Ready_Patch] 晓夫九卡池筛选按钮添加成功");
		}
		catch (Exception e)
		{
			Log.Error($"[NCardLibrary_Ready_Patch] 添加筛选按钮失败: {e.Message}");
			Log.Error(e.StackTrace);
		}
	}

	private static void TrySetIcon(NCardPoolFilter filter)
	{
		try
		{
			var imageNode = filter.GetNode<Control>("Image");
			if (imageNode == null) return;

			// 加载晓夫九图标场景
			var iconScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/character_icons/xiaofujiu_icon.tscn");
			if (iconScene == null) return;

			var iconInstance = iconScene.Instantiate<Control>();
			if (iconInstance == null) return;

			// 清除旧图标子节点，添加新图标
			foreach (var child in imageNode.GetChildren())
			{
				if (child is Node node)
					node.QueueFree();
			}
			imageNode.AddChild(iconInstance);
			iconInstance.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

			Log.Info("[TrySetIcon] 晓夫九图标设置成功");
		}
		catch (Exception e)
		{
			Log.Error($"[TrySetIcon] 设置图标失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁6：所有先古对话中注入晓夫九的对话
/// </summary>

// 各先古的晓夫九对话行数（用于创建 AncientDialogue）
public static class XiaofujiuAncientDialogues
{
	private static readonly Dictionary<string, int> _lineCounts = new()
	{
		["THE_ARCHITECT"] = 3,
		["DARV"] = 3,
		["NEOW"] = 1,
		["NONUPEIPE"] = 3,
		["OROBAS"] = 3,
		["PAEL"] = 3,
		["TANX"] = 3,
		["TEZCATARA"] = 3,
		["VAKUU"] = 3,
	};

	public static int? GetLineCount(string ancientId)
	{
		return _lineCounts.TryGetValue(ancientId, out var count) ? count : null;
	}
}

/// <summary>
/// 向 AncientDialogueSet 注入晓夫九对话的通用方法
/// </summary>
public static class AncientDialogueInjector
{
	public static void Inject(AncientDialogueSet dialogueSet, string ancientId, ArchitectAttackers endAttackers = ArchitectAttackers.None)
	{
		var lineCount = XiaofujiuAncientDialogues.GetLineCount(ancientId);
		if (lineCount == null) return;

		var charKey = ModelDb.Character<Xiaofujiu>().Id.Entry;
		if (dialogueSet.CharacterDialogues.ContainsKey(charKey)) return;

		// 创建对话：每行一个空字符串（SFX路径占位）
		var sfxPaths = Enumerable.Repeat("", lineCount.Value).ToArray();
		var dialogue = new AncientDialogue(sfxPaths)
		{
			VisitIndex = 0,
			IsRepeating = true,  // ✅ 兜底：即使 FirstVisitEverDialogue 覆盖也能在后续访问匹配
			EndAttackers = endAttackers,
		};

		// 手动调用 PopulateLines（因为 PopulateLocKeys 在 Postfix 之前已执行）
		dialogue.PopulateLines(ancientId, charKey, 0);

		// 设置非末行的 .next 按钮文本
		for (int k = 0; k < dialogue.Lines.Count - 1; k++)
		{
			var line = dialogue.Lines[k];
			var locEntryKey = line.LineText.LocEntryKey;
			var baseKey = locEntryKey.Substring(0, locEntryKey.LastIndexOf('.'));
			line.NextButtonText = new LocString("ancients", baseKey + ".next");
		}

		// 注入到 CharacterDialogues 字典
		var dict = dialogueSet.CharacterDialogues as Dictionary<string, IReadOnlyList<AncientDialogue>>;
		dict?.Add(charKey, new List<AncientDialogue> { dialogue });
	}
}

// 补丁6a：建筑师（TheArchitect 直接继承 EventModel，需单独补丁）
[HarmonyPatch(typeof(TheArchitect), nameof(TheArchitect.DialogueSet), MethodType.Getter)]
public static class TheArchitect_DialogueSet_Patch
{
	static void Postfix(ref AncientDialogueSet __result)
	{
		try
		{
			AncientDialogueInjector.Inject(__result, "THE_ARCHITECT", ArchitectAttackers.Architect);
			Log.Info("[TheArchitect_DialogueSet_Patch] 晓夫九建筑师对话注入成功");
		}
		catch (Exception e)
		{
			Log.Error($"[TheArchitect_DialogueSet_Patch] 注入失败: {e.Message}");
		}
	}
}

// 补丁6b：其余先古（Darv/Neow/Nonupeipe/Orobas/Pael/Tanx/Tezcatara/Vakuu）
[HarmonyPatch(typeof(AncientEventModel), nameof(AncientEventModel.DialogueSet), MethodType.Getter)]
public static class AncientEventModel_DialogueSet_Patch
{
	static void Postfix(AncientEventModel __instance, ref AncientDialogueSet __result)
	{
		try
		{
			var ancientId = __instance.Id.Entry;
			AncientDialogueInjector.Inject(__result, ancientId);
			Log.Info($"[AncientEventModel_DialogueSet_Patch] {ancientId} 晓夫九对话注入成功");
		}
		catch (Exception e)
		{
			Log.Error($"[AncientEventModel_DialogueSet_Patch] 注入失败: {e.Message}");
		}
	}
}

// 补丁6c：修复 GetValidDialogues 中 FirstVisitEverDialogue 优先级高于角色对话的问题
[HarmonyPatch(typeof(AncientDialogueSet), nameof(AncientDialogueSet.GetValidDialogues))]
public static class AncientDialogueSet_GetValidDialogues_Patch
{
	static bool Prefix(
		AncientDialogueSet __instance,
		ModelId characterId,
		int charVisits,
		int totalVisits,
		bool allowAnyCharacterDialogues,
		ref IEnumerable<AncientDialogue> __result)
	{
		// 仅对晓夫九生效：跳过 FirstVisitEverDialogue，优先查角色对话
		var xiaofujiuKey = ModelDb.Character<Xiaofujiu>().Id.Entry;
		if (characterId.Entry != xiaofujiuKey) return true;

		// 先查角色专属对话
		if (__instance.CharacterDialogues.TryGetValue(xiaofujiuKey, out var charDialogues))
		{
			// 精确匹配 VisitIndex
			var exact = charDialogues.Where(d => d.VisitIndex == charVisits).ToList();
			if (exact.Count > 0)
			{
				__result = exact;
				return false;
			}
			// 兜底：重复对话
			var repeating = new List<AncientDialogue>();
			foreach (var d in charDialogues)
			{
				if (d.IsRepeating && (!d.VisitIndex.HasValue || charVisits >= d.VisitIndex.Value))
					repeating.Add(d);
			}
			if (repeating.Count > 0)
			{
				__result = repeating;
				return false;
			}
		}
		return true; // 回退到原方法
	}
}

/// <summary>
/// 补丁10：将"直播之路"事件注入第一层事件池
/// </summary>
[HarmonyPatch(typeof(Underdocks), nameof(Underdocks.AllEvents), MethodType.Getter)]
public static class Underdocks_AllEvents_Patch
{
	static void Postfix(ref IEnumerable<EventModel> __result)
	{
		try
		{
			var liveStreamRoad = ModelDb.Event<LiveStreamRoad>();
			if (liveStreamRoad != null)
			{
				__result = __result.Append(liveStreamRoad);
			}
		}
		catch (Exception e)
		{
			Log.Error($"[Underdocks_AllEvents_Patch] 注入失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁10b：直播之路固定出现在玩家第一层遇到的第一个问号中
/// </summary>
public static class LiveStreamFirstUnknownPatch
{
	private static MegaCrit.Sts2.Core.Runs.RunState? _trackedRun;
	private static bool _firstUnknownDone;

	private static void EnsureTracked(MegaCrit.Sts2.Core.Runs.RunState? state)
	{
		if (state != null && !ReferenceEquals(_trackedRun, state))
		{
			_trackedRun = state;
			_firstUnknownDone = false;
		}
	}

	// 从 run 的玩家身上拿幸运房东遗物（其 LiveStreamGiven 是 [SavedProperty]，SL 后不丢）
	private static LuckyLandlordRelic? GetLandlord(MegaCrit.Sts2.Core.Runs.RunState? state)
	{
		if (state == null) return null;
		return state.Players?.FirstOrDefault()?.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
	}

	private static bool IsLiveStreamGiven(MegaCrit.Sts2.Core.Runs.RunState? state)
	{
		return GetLandlord(state)?.LiveStreamGiven ?? false;
	}

	[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Runs.RunManager), "RollRoomTypeFor")]
	public static class RollRoomTypeFor_Patch
	{
		static void Postfix(MegaCrit.Sts2.Core.Runs.RunManager __instance, MegaCrit.Sts2.Core.Map.MapPointType pointType, ref MegaCrit.Sts2.Core.Rooms.RoomType __result)
		{
			try
			{
				var state = HarmonyLib.Traverse.Create(__instance).Property("State").GetValue<MegaCrit.Sts2.Core.Runs.RunState>();
				EnsureTracked(state);
				if (state != null && state.CurrentActIndex == 0
					&& pointType == MegaCrit.Sts2.Core.Map.MapPointType.Unknown && !_firstUnknownDone
					&& !IsLiveStreamGiven(state))
				{
					// 第一层第一个问号强制为事件（直播之路已给过则不再强制）
					_firstUnknownDone = true;
					__result = MegaCrit.Sts2.Core.Rooms.RoomType.Event;
				}
			}
			catch (Exception e)
			{
				Log.Error($"[RollRoomTypeFor_Patch] 失败: {e.Message}");
			}
		}
	}

	[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.ActModel), "PullNextEvent")]
	public static class PullNextEvent_Patch
	{
		static bool Prefix(MegaCrit.Sts2.Core.Models.ActModel __instance, MegaCrit.Sts2.Core.Runs.RunState runState, ref MegaCrit.Sts2.Core.Models.EventModel __result)
		{
			try
			{
				EnsureTracked(runState);
				if (runState.CurrentActIndex == 0 && !IsLiveStreamGiven(runState))
				{
					// 第一层第一个事件 = 直播之路（只给一次；用遗物持久化标志防 SL 后重复）
					// 注意：EventRoom 要求 canonical 模型，不能 ToMutable
					var landlord = GetLandlord(runState);
					if (landlord != null)
					{
						landlord.LiveStreamGiven = true;
					}
					// 标记为已访问：原方法会 AddVisitedEvent 但被本 Prefix 跳过；
					// 不记录的话 EnsureNextEventIsValid 的 visited 过滤无法排除本事件，
					// 事件轮转（跳过其它已访问事件）时本事件会被再次选中 → 一局重复出现
					var liveEvent = ModelDb.Event<LiveStreamRoad>();
					runState.AddVisitedEvent(liveEvent);
					__result = liveEvent;
					return false;
				}
			}
			catch (Exception e)
			{
				Log.Error($"[PullNextEvent_Patch] 失败: {e.Message}");
			}
			return true;
		}
	}

	/// <summary>
	/// 补丁：每场战斗开始重置本场转化计数（修复入夫门 SL 后统计整局转化次数）
	/// </summary>
	[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Combat.CombatManager), "StartCombatInternal")]
	public static class CombatManager_StartCombatInternal_Patch
	{
		static void Prefix()
		{
			XiaofujiuCardBase.ResetCombatConvertCounts();
		}
	}

	/// <summary>
	/// 补丁：每次进入战斗，第一个怪物随机说一句夫黄SC（使用游戏已有对话气泡）
	/// </summary>
	[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Combat.CombatManager), "SetUpCombat")]
	public static class CombatManager_SetUpCombat_SpeechPatch
	{
		private static readonly string[] _speechLines = new string[]
		{
			"想要夫哥开灵动步伐对着我的创世之柱展示非凡技艺",
			"夫哥哥我来吃你的扭牛了想我了吗",
			"妈妈妈妈妈妈妈妈妈妈妈妈妈妈吗妈妈",
			"夫哥可以往后坐一坐吗，我有点喘不上气",
			"夫夫，你昨天晚上蒙住我的眼睛说请我吃小辣椒，呛得我嗓子疼",
			"夫哥你辣边辣个摄像头掉了，你扶一下",
			"夫哥我家里的收音机坏了，你给我收音机吧",
			"夫哥可以对着我的咕嘟冒泡的创世之柱使用贪婪之手吗",
			"WWWWWWWWWWWWWWWWWW",
			"夫哥能不能多解几个扣子，有急事",
			"夫哥你这里怎么没有帮我玩服务？上舰了能不能帮我玩？",
			"夫哥打牌太粗了，看得我都想替夫哥玩几把了",
			"好想把我作为祭品让夫哥主宰我的巨像一直倾泻到放血最好时不时凶恶一下",
			"夫哥，你像个廉价挑单般闯入我的生活让我陷入高潮一般梦幻的时间结果又突然漏电炸波一",
		};

		// 注意：Harmony 注入原方法参数要求参数名一致（原方法签名 SetUpCombat(CombatState state)）
		static void Postfix(MegaCrit.Sts2.Core.Combat.CombatState state)
		{
			try
			{
				// fuhuangsc on/off 指令控制开关
				if (!kyxiaofujiu.utils.FuHuangScToggle.Enabled) return;
				// 只有第一个存活的怪物说
				var enemy = state.Enemies.FirstOrDefault(c => !c.IsDead);
				if (enemy == null) return;
				var text = _speechLines[System.Random.Shared.Next(_speechLines.Length)];
				// 战斗开始后 1 秒再播，避免被开场/战斗开始提示盖住
				_ = SpeakDelayed(enemy, text);
			}
			catch (Exception e)
			{
				Log.Error($"[CombatManager_SetUpCombat_SpeechPatch] 失败: {e.Message}");
			}
		}

		// 延迟 1 秒在敌人头顶弹出语音气泡（异步续体回主线程；敌人死亡/战斗结束时静默跳过）
		private static async System.Threading.Tasks.Task SpeakDelayed(MegaCrit.Sts2.Core.Entities.Creatures.Creature enemy, string text)
		{
			try
			{
				await System.Threading.Tasks.Task.Delay(1000);
				if (enemy == null || enemy.IsDead) return;
				if (enemy.GetVfxContainer() == null) return;
				// 按文本长度计算显示时长（模仿 TalkCmd）
				double seconds = System.Math.Max(2.0, text.Length * 0.12);
				var bubble = MegaCrit.Sts2.Core.Nodes.Vfx.NSpeechBubbleVfx.Create(text, enemy, seconds);
				enemy.GetVfxContainer()?.AddChildSafely(bubble);
			}
			catch (Exception e)
			{
				Log.Error($"[CombatManager_SetUpCombat_SpeechPatch] 语音延迟失败: {e.Message}");
			}
		}
	}

	/// <summary>
	/// 补丁10c：将"联机"事件注入第二层事件池，"BW之夜"注入第三层事件池
	/// </summary>
	[HarmonyPatch(typeof(Hive), nameof(Hive.AllEvents), MethodType.Getter)]
	public static class Hive_AllEvents_Patch
	{
		static void Postfix(ref IEnumerable<EventModel> __result)
		{
			try
			{
				var lianji = ModelDb.Event<LianJi>();
				if (lianji != null)
				{
					__result = __result.Append(lianji);
				}
			}
			catch (Exception e)
			{
				Log.Error($"[Hive_AllEvents_Patch] 注入失败: {e.Message}");
			}
		}
	}

	[HarmonyPatch(typeof(Glory), nameof(Glory.AllEvents), MethodType.Getter)]
	public static class Glory_AllEvents_Patch
	{
		static void Postfix(ref IEnumerable<EventModel> __result)
		{
			try
			{
				var bwNight = ModelDb.Event<BwNight>();
				if (bwNight != null)
				{
					__result = __result.Append(bwNight);
				}
			}
			catch (Exception e)
			{
				Log.Error($"[Glory_AllEvents_Patch] 注入失败: {e.Message}");
			}
		}
	}

	/// <summary>
	/// 补丁10d：第二层第一个事件=联机；第三层第一个事件=BW之夜（无灯火钥匙时；有钥匙则战史学家后刷新）
	/// 模仿直播之路写法：RollRoomTypeFor 把每层第一个问号强制为事件房间，PullNextEvent 再把事件替换成目标事件。
	/// </summary>
	public static class ActEventsFirstPatch
	{
		private static MegaCrit.Sts2.Core.Runs.RunState? _trackedRun;
		private static bool _secondRoomDone;   // 二层第一个问号已强制为事件房间
		private static bool _thirdRoomDone;    // 三层第一个问号已强制为事件房间
		private static bool _secondGiven;      // 二层联机已给（静态兜底，防无房东遗物时重复）
		private static bool _thirdGiven;       // 三层BW之夜已给（静态兜底）

		private static void EnsureTracked(MegaCrit.Sts2.Core.Runs.RunState? state)
		{
			if (state != null && !ReferenceEquals(_trackedRun, state))
			{
				_trackedRun = state;
				_secondRoomDone = false;
				_thirdRoomDone = false;
				_secondGiven = false;
				_thirdGiven = false;
			}
		}

		private static LuckyLandlordRelic? GetLandlord(MegaCrit.Sts2.Core.Runs.RunState? state)
		{
			if (state == null) return null;
			return state.Players?.FirstOrDefault()?.Relics.OfType<LuckyLandlordRelic>().FirstOrDefault();
		}

		private static bool HasLanternKey(MegaCrit.Sts2.Core.Runs.RunState state)
		{
			return state.Players?.Any(p => p.Deck.Cards.Any(c => c is LanternKey)) ?? false;
		}

		// 二层/三层第一个问号强制为事件房间（模仿直播之路 RollRoomTypeFor_Patch）
		[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Runs.RunManager), "RollRoomTypeFor")]
		public static class RollRoomTypeFor_ActPatch
		{
			static void Postfix(MegaCrit.Sts2.Core.Runs.RunManager __instance, MegaCrit.Sts2.Core.Map.MapPointType pointType, ref MegaCrit.Sts2.Core.Rooms.RoomType __result)
			{
				try
				{
					var state = HarmonyLib.Traverse.Create(__instance).Property("State").GetValue<MegaCrit.Sts2.Core.Runs.RunState>();
					EnsureTracked(state);
					if (state == null) return;
					// 二层第一个问号 → 事件房间（联机）
					if (state.CurrentActIndex == 1 && pointType == MegaCrit.Sts2.Core.Map.MapPointType.Unknown && !_secondRoomDone && GetLandlord(state)?.LianJiGiven != true)
					{
						_secondRoomDone = true;
						__result = MegaCrit.Sts2.Core.Rooms.RoomType.Event;
					}
					// 三层第一个问号 → 事件房间（BW之夜，卡组无灯火钥匙时）
					else if (state.CurrentActIndex == 2 && pointType == MegaCrit.Sts2.Core.Map.MapPointType.Unknown && !_thirdRoomDone && GetLandlord(state)?.BwNightGiven != true && !HasLanternKey(state))
					{
						_thirdRoomDone = true;
						__result = MegaCrit.Sts2.Core.Rooms.RoomType.Event;
					}
				}
				catch (Exception e)
				{
					Log.Error($"[RollRoomTypeFor_ActPatch] 失败: {e.Message}");
				}
			}
		}

		[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.ActModel), "PullNextEvent")]
		public static class PullNextEvent_ActPatch
		{
			static bool Prefix(MegaCrit.Sts2.Core.Models.ActModel __instance, MegaCrit.Sts2.Core.Runs.RunState runState, ref MegaCrit.Sts2.Core.Models.EventModel __result)
			{
				try
				{
					EnsureTracked(runState);
					// 第二层：未给过联机 → 第一个事件 = 联机（遗物持久化标志防 SL 重复 + 静态兜底）
					if (runState.CurrentActIndex == 1 && !_secondGiven && GetLandlord(runState)?.LianJiGiven != true)
					{
						_secondGiven = true;
						if (GetLandlord(runState) is { } l) l.LianJiGiven = true;
						// 标记 visited：与直播之路同理，防止事件轮转时再次被选中
						var lianjiEvent = ModelDb.Event<LianJi>();
						runState.AddVisitedEvent(lianjiEvent);
						__result = lianjiEvent;
						return false;
					}
					// 第三层：未给过BW之夜且卡组无灯火钥匙 → 第一个事件 = BW之夜
					// 有灯火钥匙：放行原逻辑（LanternKey.ModifyNextEvent → 战史学家），
					// 战史学家会移除灯火钥匙，之后这里卡组无钥匙 → BW之夜
					if (runState.CurrentActIndex == 2 && !_thirdGiven && GetLandlord(runState)?.BwNightGiven != true && !HasLanternKey(runState))
					{
						_thirdGiven = true;
						if (GetLandlord(runState) is { } l) l.BwNightGiven = true;
						// 标记 visited：防止事件轮转时再次被选中（三层原版事件少，最易触发二次）
						var bwEvent = ModelDb.Event<BwNight>();
						runState.AddVisitedEvent(bwEvent);
						__result = bwEvent;
						return false;
					}
				}
				catch (Exception e)
				{
					Log.Error($"[PullNextEvent_ActPatch] 失败: {e.Message}");
				}
				return true;
			}
		}
	}
}

/// <summary>
/// 补丁7：怪物对话支持角色专属文本（XIAOFUJIU 后缀）
/// 用法：在 monsters.json 中添加 {原key}.XIAOFUJIU 即可覆盖
/// </summary>
[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.L10NMonsterLookup))]
public static class MonsterModel_L10NMonsterLookup_Patch
{
	static void Postfix(string entryName, ref LocString __result)
	{
		try
		{
			var charKey = entryName + ".XIAOFUJIU";
			if (LocString.Exists("monsters", charKey))
			{
				__result = new LocString("monsters", charKey);
			}
		}
		catch (Exception e)
		{
			Log.Error($"[MonsterModel_L10NMonsterLookup_Patch] 失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁8：TalkCmd.Play 通用角色专属对话（支持 monsters.json 和 powers.json）
/// 用法：在任意 loc 表中添加 {原key}.XIAOFUJIU 即可覆盖
/// </summary>
[HarmonyPatch(typeof(TalkCmd), nameof(TalkCmd.Play))]
public static class TalkCmd_Play_Patch
{
	static void Prefix(ref LocString line, Creature speaker)
	{
		try
		{
			// 检查是否晓夫九对局
			var players = speaker.CombatState?.Players;
			if (players == null) return;
			var isXiaofujiu = players.Any(p => p.Character is Xiaofujiu);
			if (!isXiaofujiu) return;

			// 检查是否有 .XIAOFUJIU 变体
			var charKey = line.LocEntryKey + ".XIAOFUJIU";
			if (LocString.Exists(line.LocTable, charKey))
			{
				line = new LocString(line.LocTable, charKey);
			}
		}
		catch (Exception e)
		{
			Log.Error($"[TalkCmd_Play_Patch] 失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁9：征婚时累计婚戒计数器
/// </summary>
[HarmonyPatch(typeof(ZhengHunCmd), nameof(ZhengHunCmd.Marry))]
public static class ZhengHunCmd_Marry_Patch
{
	static async Task Postfix(Task __result, PlayerChoiceContext choiceContext, Player player, int amount)
	{
		await __result;
		try
		{
			if (player == null || amount <= 0) return;
			var ring = player.GetRelic<WeddingRing>();
			if (ring != null)
			{
				await ring.IncrementMarry(amount);
			}
		}
		catch (Exception e)
		{
			Log.Error($"[ZhengHunCmd_Marry_Patch] 失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁9.1：SpinReward（旋转一次）SL 恢复兜底。
/// 游戏 Reward.FromSerializable 对 RewardType.None 抛 NotImplementedException，
/// 拦截 None 类型：从玩家遗物取幸运房东，返回 SpinReward。
/// </summary>
[HarmonyPatch(typeof(Reward), nameof(Reward.FromSerializable))]
public static class Reward_FromSerializable_SpinReward_Patch
{
	static bool Prefix(SerializableReward save, Player player, ref Reward __result)
	{
		if (save.RewardType != RewardType.None) return true;  // 非旋转奖励，走原逻辑
		var relic = player.GetRelic<LuckyLandlordRelic>();
		if (relic == null) return true;  // 理论不会发生（遗物 SL 保留）
		__result = new SpinReward(player, relic);
		return false;  // 跳过原方法
	}
}

/// <summary>
/// 补丁10：夫黑/夫黄状态作为卡面关键字词条显示（像"保留""消耗""虚无"一样）
/// 在卡面描述前插入 [gold]夫黑/夫黄[/gold]。词条；夫白不显示（默认状态）
/// 词条与游戏自带的"保留"（夫黑/夫黄时）词条并列，效果由 FufuState 驱动
/// </summary>
[HarmonyPatch]
public static class CardModel_GetDescriptionForPile_FufuTag_Patch
{
	static MethodBase TargetMethod()
	{
		var previewType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic);
		return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile",
			new Type[] { typeof(PileType), previewType, typeof(Creature) });
	}

	static void Postfix(CardModel __instance, ref string __result)
	{
		try
		{
			if (__instance is not XiaofujiuCardBase fufuCard) return;

			string tag;
			switch (fufuCard.FufuState)
			{
				case FufuState.Black: tag = "[gold]夫黑[/gold]。"; break;
				case FufuState.Yellow: tag = "[gold]夫黄[/gold]。"; break;
				default: return; // 夫白/无夫态不显示
			}

			__result = string.IsNullOrEmpty(__result) ? tag : tag + "\n" + __result;
		}
		catch (Exception e)
		{
			Log.Error($"[CardModel_GetDescriptionForPile_FufuTag_Patch] 失败: {e.Message}");
		}
	}
}

/// <summary>
/// 回合结束阶段标志：patch CombatManager.DoTurnEnd 开始时置 true，
/// 让"一次一个"（YiciyigePower）知道当前在回合结束流程，抽的牌保留（不立即被弃）。
/// 清除放在 EndPlayerTurnPhaseTwoInternal（FlushPlayerHand 弃牌判断之后），
/// 因为一次一个抽牌是异步的，DoTurnEnd 结束就清除会有竞态。
/// 场景：要嫁就嫁（夫黑）回合结束转化打出 → 触发一次一个抽牌 → 抽的牌应保留在手牌
/// </summary>
[HarmonyPatch(typeof(CombatManager), "DoTurnEnd")]
public static class CombatManager_DoTurnEnd_Patch
{
	static void Prefix()
	{
		kyxiaofujiu.powers.YiciyigePower.RetainDrawnAtCombatEnd = true;
	}
}

/// <summary>
/// FlushPlayerHand（弃牌判断）完成后清除回合结束保留标志
/// ⚠️ CombatManager.EndPlayerTurnPhaseTwoInternal 有 2 个重载（无参/带 CombatTurnState），
/// 必须精确定位带参重载（CombatTurnState 是 internal，用反射），否则 Harmony 报 Ambiguous
/// </summary>
[HarmonyPatch]
public static class CombatManager_EndPlayerTurnPhaseTwoInternal_Patch
{
	static MethodBase TargetMethod()
	{
		var paramType = typeof(CombatManager).Assembly.GetType("MegaCrit.Sts2.Core.Combat.CombatTurnState");
		return AccessTools.Method(typeof(CombatManager), "EndPlayerTurnPhaseTwoInternal", new Type[] { paramType });
	}

	static void Postfix(Task __result)
	{
		// 用 ContinueWith 避免 async void
		__result?.ContinueWith(_ => kyxiaofujiu.powers.YiciyigePower.RetainDrawnAtCombatEnd = false);
	}
}

/// <summary>
/// 补丁11：将"白区"Boss 注入第三层（Glory）Boss 池
/// </summary>
[HarmonyPatch(typeof(Glory), nameof(Glory.BossDiscoveryOrder), MethodType.Getter)]
public static class Glory_BossDiscoveryOrder_Patch
{
	static void Postfix(ref IEnumerable<EncounterModel> __result)
	{
		try
		{
			var baoQu = ModelDb.Encounter<BaoQuBoss>();
			if (baoQu != null)
			{
				__result = __result.Append(baoQu);
			}
		}
		catch (Exception e)
		{
			Log.Error($"[Glory_BossDiscoveryOrder_Patch] 注入失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁11b：将"白区"Boss 加入 Glory 的所有遭遇列表（随机Boss池）
/// </summary>
[HarmonyPatch(typeof(Glory), nameof(Glory.GenerateAllEncounters))]
public static class Glory_GenerateAllEncounters_Patch
{
	static void Postfix(ref IEnumerable<EncounterModel> __result)
	{
		try
		{
			var baoQu = ModelDb.Encounter<BaoQuBoss>();
			if (baoQu != null)
			{
				__result = __result.Append(baoQu);
			}
		}
		catch (Exception e)
		{
			Log.Error($"[Glory_GenerateAllEncounters_Patch] 注入失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁12：Boss 战斗自定义音乐（要嫁就嫁晓夫九）
/// </summary>
[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.PlayCustomMusic))]
public static class NRunMusicController_PlayCustomMusic_Patch
{
	static bool Prefix(NRunMusicController __instance, string customMusic)
	{
		try
		{
			if (customMusic != null && customMusic.StartsWith("custom:"))
			{
				// 停止原 FMOD 音乐，改用 mod 提供的 mp3 循环播放
				__instance.StopMusic();
				CustomAudioManager.PlayBgm("res://audio/幻琉 - 要嫁就嫁晓夫九(AI白夕填词).mp3");
				return false;
			}
		}
		catch (Exception e)
		{
			Log.Error($"[PlayCustomMusic_Patch] 失败: {e.Message}");
		}
		return true;
	}
}

/// <summary>
/// 补丁12b：停止音乐时同时停止自定义BGM
/// </summary>
[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.StopMusic))]
public static class NRunMusicController_StopMusic_Patch
{
	static void Postfix()
	{
		try
		{
			CustomAudioManager.StopBgm();
		}
		catch (Exception e)
		{
			Log.Error($"[StopMusic_Patch] 失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁12c：切换房间音乐时同时停止自定义BGM
/// </summary>
[HarmonyPatch(typeof(NRunMusicController), nameof(NRunMusicController.UpdateMusic))]
public static class NRunMusicController_UpdateMusic_Patch
{
	static void Postfix()
	{
		try
		{
			CustomAudioManager.StopBgm();
		}
		catch (Exception e)
		{
			Log.Error($"[UpdateMusic_Patch] 失败: {e.Message}");
		}
	}
}

/// <summary>
/// 补丁13：白区Boss战动态添加婚礼背景（纯代码，绕开场景文件与pck场景编译）
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.SetUpBackground))]
public static class NCombatRoom_SetUpBackground_Patch
{
	static void Postfix(NCombatRoom __instance)
	{
		try
		{
			if (__instance == null || __instance.Background == null) return;

			// 仅对白区Boss生效
			var field = typeof(NCombatRoom).GetField("_visuals", BindingFlags.NonPublic | BindingFlags.Instance);
			var visuals = field?.GetValue(__instance) as ICombatRoomVisuals;
			if (visuals == null || visuals.Encounter is not BaoQuBoss) return;

			// 加到背景容器最顶层，覆盖默认背景
			CustomBackgroundManager.ShowBossBackground(__instance.Background.GetParent());
		}
		catch (Exception e)
		{
			Log.Error($"[NCombatRoom_SetUpBackground_Patch] 失败: {e.Message}");
		}
	}
}



}
