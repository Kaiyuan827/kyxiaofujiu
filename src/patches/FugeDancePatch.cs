using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace kyxiaofujiu.patches
{
	/// <summary>
	/// 战斗 UI 激活时，在屏幕左上角挂一个循环播放的"夫哥跳舞"动画。
	/// </summary>
	[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
	public static class NCombatUi_Activate_Dance_Patch
	{
		private static AnimatedSprite2D? _dance;

		private static void Postfix(NCombatUi __instance)
		{
			try
			{
				if (_dance == null || !GodotObject.IsInstanceValid(_dance))
				{
					var frames = ResourceLoader.Load<SpriteFrames>("res://animations/fuge_dance_frames.tres");
					if (frames == null)
					{
						GD.PushWarning("[kyxiaofujiu] fuge_dance_frames.tres missing");
						return;
					}
					_dance = new AnimatedSprite2D
					{
						Name = "FugeDance",
						SpriteFrames = frames,
						Animation = "dance",
						Autoplay = "dance",
					};
					float scale = 160f / 720f;
					_dance.Scale = new Vector2(scale, scale);
					_dance.Position = new Vector2(24f + 251f * scale, 24f + 360f);
					_dance.ZIndex = 50;
				}
				if (_dance.GetParent() == null)
				{
					__instance.AddChild(_dance);
				}
				else if (_dance.GetParent() != __instance)
				{
					_dance.GetParent().RemoveChild(_dance);
					__instance.AddChild(_dance);
				}
				_dance.Visible = true;
				_dance.Play("dance");
			}
			catch (System.Exception e)
			{
				GD.PushWarning("[kyxiaofujiu] dance patch failed: " + e.Message);
			}
		}
	}
}
