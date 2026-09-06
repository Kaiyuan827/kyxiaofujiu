using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace kyxiaofujiu
{
	/// <summary>
	/// 兼容模式：防止与其他 mod 冲突。
	/// 配置文件 user://kyxiaofujiu_config.json 的 "compatibilityMode": true 时，
	/// 只应用"人物选择/角色基础"补丁（角色仍可正常选择），其余机制补丁（动画/事件/
	/// Boss/征婚/工资/对话等）不加载。改动重启生效。
	/// 配置文件示例：{ "compatibilityMode": true }
	/// </summary>
	public static class XiaofujiuCompat
	{
		public static bool CompatibilityMode { get; private set; }

		public const string HarmonyId = "kyxiaofujiu.xiaofujiu";

		private const string ConfigFileName = "kyxiaofujiu_config.json";

		// 兼容模式下仍应用的补丁（人物选择/角色基础：保证角色可选、可见、卡池/遗物池正常）
		private static readonly HashSet<string> EssentialPatches = new()
		{
			"ModelDbAllCharactersPatch",            // 角色出现在选择界面
			"ModelDbAllCardPoolsPatch",             // 卡池
			"ModelDbAllRelicPoolsPatch",            // 遗物池
			"CharacterModel_EnergyCounterPath_Patch", // 角色能量计数视觉
			"CharacterModel_TrailPath_Patch",       // 角色足迹视觉
			"ModelIdSerializationCache_Init_Patch", // 模型序列化缓存（角色数据/联机）
			"NCardLibrary_Ready_Patch",             // 卡牌大全显示 mod 卡
			"EpochModel_Get_Patch",                 // 角色的 epoch 注册（XIAOFUJIU2_EPOCH）
			"EpochModel_AllEpochIds_Patch",         // 列出角色的 epoch（缺了黑屏）
		};

		private static string ConfigFilePath
		{
			get
			{
				try
				{
					return Path.Combine(OS.GetUserDataDir(), ConfigFileName);
				}
				catch
				{
					return ConfigFileName;
				}
			}
		}

		/// <summary>
		/// 查找配置文件：优先 mod DLL 所在目录（游戏 mods 部署目录 / 本地 build 目录），
		/// 其次游戏用户数据目录（与日志同层 AppData\Roaming\SlayTheSpire2）。
		/// 找到返回路径，否则返回 null。
		/// </summary>
		private static string? FindConfigFile()
		{
			try
			{
				// 1) mod DLL 所在目录（最常见：游戏 mods 目录里 kyxiaofujiu.dll 旁边）
				string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
				string p1 = Path.Combine(dllDir, ConfigFileName);
				if (File.Exists(p1)) return p1;

				// 2) 游戏用户数据目录（与 godot.log 同层）
				string p2 = Path.Combine(OS.GetUserDataDir(), ConfigFileName);
				if (File.Exists(p2)) return p2;
			}
			catch (Exception e)
			{
				Log.Warn($"[XiaofujiuCompat] 查找配置失败: {e.Message}");
			}
			return null;
		}

		public static void Load()
		{
			CompatibilityMode = false;
			try
			{
				string? path = FindConfigFile();
				if (path != null && File.Exists(path))
				{
					var json = JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(path));
					if (json != null && json.TryGetValue("compatibilityMode", out bool v))
					{
						CompatibilityMode = v;
					}
					Log.Info($"[XiaofujiuCompat] 读取配置: {path}");
				}
				else
				{
					Log.Info($"[XiaofujiuCompat] 未找到配置文件，默认正常模式（可放 {ConfigFilePath} 或 mod 目录的 {ConfigFileName}）");
				}
			}
			catch (Exception e)
			{
				Log.Warn($"[XiaofujiuCompat] 读取配置失败（默认正常模式）: {e.Message}");
			}
			Log.Info($"[XiaofujiuCompat] 兼容模式: {(CompatibilityMode ? "开启" : "关闭")}");
		}

		/// <summary>
		/// 应用补丁：兼容模式开启时只应用人物选择/角色基础补丁，其余跳过。
		/// 单个补丁失败不中断（记录错误继续），避免某个歧义/失败补丁导致后续全部不生效
		/// </summary>
		public static void ApplyPatches(Harmony harmony)
		{
			int applied = 0;
			int skipped = 0;
			int failed = 0;
			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (type.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0) continue;

				if (CompatibilityMode && !EssentialPatches.Contains(type.Name))
				{
					Log.Info($"[XiaofujiuCompat] 兼容模式：跳过补丁 {type.Name}");
					skipped++;
					continue;
				}

				try
				{
					harmony.CreateClassProcessor(type).Patch();
					applied++;
				}
				catch (Exception e)
				{
					Log.Error($"[XiaofujiuCompat] 补丁应用失败 {type.Name}: {e.Message}");
					failed++;
				}
			}
			Log.Info($"[XiaofujiuCompat] 补丁应用 {applied} 个，兼容模式跳过 {skipped} 个，失败 {failed} 个");
		}

		// 配置写入路径：优先 mod DLL 所在目录（游戏 mods 目录），fallback 用户数据目录
		private static string ConfigWritePath
		{
			get
			{
				try
				{
					string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
					return Path.Combine(dllDir, ConfigFileName);
				}
				catch
				{
					return ConfigFileName;
				}
			}
		}

		public static void SaveConfig()
		{
			try
			{
				string path = ConfigWritePath;
				File.WriteAllText(path, JsonSerializer.Serialize(new Dictionary<string, bool> { ["compatibilityMode"] = CompatibilityMode }));
				Log.Info($"[XiaofujiuCompat] 已保存配置: {path}");
			}
			catch (Exception e)
			{
				Log.Warn($"[XiaofujiuCompat] 保存配置失败: {e.Message}");
			}
		}

		/// <summary>
		/// 运行时切换兼容模式：写配置文件 + 卸载本 mod 全部补丁后按新模式重新应用。
		/// ⚠️ 建议在主菜单/非战斗时调用（运行中 Unpatch 游戏核心方法有风险）。
		/// </summary>
		public static void SetCompatibilityMode(bool on)
		{
			if (CompatibilityMode == on) return;
			CompatibilityMode = on;
			SaveConfig();

			try
			{
				var harmony = new Harmony(HarmonyId);
				harmony.UnpatchAll(HarmonyId);
				ApplyPatches(harmony);
				Log.Info($"[XiaofujiuCompat] 已切换兼容模式: {(on ? "开启" : "关闭")}");
			}
			catch (Exception e)
			{
				Log.Error($"[XiaofujiuCompat] 切换补丁失败: {e.Message}");
			}
		}
	}
}
