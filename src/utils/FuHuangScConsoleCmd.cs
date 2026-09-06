using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace kyxiaofujiu.utils
{
	/// <summary>
	/// 夫黄SC 功能开关（fuhuangsc on/off 指令控制，默认开启）
	/// </summary>
	public static class FuHuangScToggle
	{
		public static bool Enabled = true;
	}

	/// <summary>
	/// 指令：fuhuangsc on/off —— 启用/禁用进入战斗时第一个怪物说夫黄SC
	/// </summary>
	public sealed class FuHuangScConsoleCmd : AbstractConsoleCmd
	{
		public override string CmdName => "fuhuangsc";

		public override string Args => "<on|off>";

		public override string Description => "启用/禁用进入战斗时第一个怪物说夫黄SC。";

		public override bool IsNetworked => false;

		// 非调试模式也可用
		public override bool DebugOnly => false;

		public override CmdResult Process(Player? issuingPlayer, string[] args)
		{
			if (args.Length != 1)
			{
				return new CmdResult(success: false, "用法: fuhuangsc on/off");
			}
			switch (args[0].ToLowerInvariant())
			{
				case "on":
					FuHuangScToggle.Enabled = true;
					return new CmdResult(success: true, "夫黄SC 已启用");
				case "off":
					FuHuangScToggle.Enabled = false;
					return new CmdResult(success: true, "夫黄SC 已禁用");
				default:
					return new CmdResult(success: false, "参数必须是 on 或 off");
			}
		}
	}
}
