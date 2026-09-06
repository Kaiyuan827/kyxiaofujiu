using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace kyxiaofujiu.utils
{
	/// <summary>
	/// 动态战斗背景管理：纯代码添加/移除婚礼背景，绕开场景文件与 pck 场景编译问题。
	/// </summary>
	public static class CustomBackgroundManager
	{
		private static TextureRect? _bgRect;

		/// <summary>
		/// 在 Boss 战斗房间显示婚礼背景。
		/// 加到背景容器（BgContainer），锚定容器中心 + 大范围偏移覆盖全屏；
		/// z_index 保持 0（与默认背景/怪物同层，靠树顺序在默认背景之上、怪物之下）。
		/// </summary>
		public static void ShowBossBackground(Node bgContainer)
		{
			HideBossBackground();

			var tex = ResourceLoader.Load<Texture2D>("res://images/backgrounds/wedding_bg.png");
			if (tex == null)
			{
				Log.Error("CustomBackgroundManager: 无法加载婚礼背景图 res://images/backgrounds/wedding_bg.png");
				return;
			}

			var rect = new TextureRect
			{
				Texture = tex,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};

			// 锚定中心，偏移覆盖全屏（参照游戏默认背景图层：±1440 x ±675）；整体左移20
			rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
			rect.OffsetLeft = -980;
			rect.OffsetRight = 940;
			rect.OffsetTop = -540;
			rect.OffsetBottom = 540;

			bgContainer.AddChild(rect);
			rect.MoveToFront();
			_bgRect = rect;

			Log.Info($"CustomBackgroundManager: 已添加婚礼背景 tex={tex.GetSize()} containerSize={(bgContainer as Control)?.Size} rect={rect.Size}");
		}

		/// <summary>
		/// 移除自定义背景
		/// </summary>
		public static void HideBossBackground()
		{
			if (_bgRect != null)
			{
				if (GodotObject.IsInstanceValid(_bgRect))
				{
					_bgRect.QueueFree();
				}
				_bgRect = null;
			}
		}
	}
}
