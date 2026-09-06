using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace kyxiaofujiu
{
	/// <summary>
	/// 控制台命令：kyxiaofujiu on|off —— 快速开关晓夫九功能
	/// on  = 正常模式（开启晓夫九全部补丁/功能）
	/// off = 兼容模式（关闭晓夫九机制补丁，只保留人物选择，防止与其他 mod 冲突）
	/// 由游戏 DevConsole 自动注册（ReflectionHelper.GetSubtypesInMods 扫描 mod 的 AbstractConsoleCmd 子类）
	/// </summary>
	public class KyxiaofujiuCmd : AbstractConsoleCmd
	{
		public override string CmdName => "kyxiaofujiu";

		public override string Args => "on|off";

		public override string Description => "快速开关晓夫九功能（on=开启全部/off=兼容模式防冲突），建议主菜单使用";

		public override bool IsNetworked => false;

		// 非 debug-only：控制台可用即注册（其他 mod 命令大多 DebugOnly=true 需 debug 权限）
		public override bool DebugOnly => false;

		public override CmdResult Process(Player? issuingPlayer, string[] args)
		{
			if (args == null || args.Length < 1)
			{
				string current = XiaofujiuCompat.CompatibilityMode ? "兼容模式（晓夫九已关闭）" : "正常模式（晓夫九已开启）";
				return new CmdResult(false, $"用法: kyxiaofujiu on|off（当前: {current}）");
			}

			switch (args[0].ToLowerInvariant())
			{
				case "on":
					// on = 开启晓夫九：正常模式（全部补丁）
					XiaofujiuCompat.SetCompatibilityMode(false);
					return new CmdResult(true, "晓夫九功能已开启（正常模式，全部补丁）。已即时生效并保存。");
				case "off":
					// off = 关闭晓夫九：兼容模式（只保留人物选择补丁，防冲突）
					XiaofujiuCompat.SetCompatibilityMode(true);
					return new CmdResult(true, "晓夫九已关闭（兼容模式：仅保留人物选择相关补丁，防冲突）。已即时生效并保存。");
				default:
					return new CmdResult(false, "无效参数。用法: kyxiaofujiu on|off");
			}
		}
	}
}
