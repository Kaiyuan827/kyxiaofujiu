using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Cards;
using kyxiaofujiu.core;

namespace kyxiaofujiu.utils
{
	public static class FufuOverlayHelper
	{
		private const float IconSize = 96f;

		// 缓存夫态图标纹理，避免每次 UpdateVisuals 都重新 ResourceLoader.Load（卡顿优化）
		private static Texture2D? _yellowTexture;
		private static Texture2D? _blackTexture;
		private static Texture2D? _whiteTexture;

		public static void Clear(NCard cardNode)
		{
			if (cardNode == null) return;

			var container = cardNode.OverlayContainer;
			if (container == null) return;

			// 立即从容器中移除旧图标（QueueFree 是延迟到下一帧才删除，
			// 转化时同帧内多次 Add 会导致旧图标残留、新图标叠加）
			foreach (var child in container.GetChildren())
			{
				if (child.Name == "FufuOverlay")
				{
					container.RemoveChild(child);
					child.QueueFree();
				}
			}
		}

		public static void Add(NCard cardNode, XiaofujiuCardBase baseCard)
		{
			if (cardNode == null || baseCard == null) return;
			if (!baseCard.IsFufu) return;

			var container = cardNode.OverlayContainer;
			if (container == null) return;

			// 幂等优化：容器中已有夫态图标则直接跳过，避免每次 UpdateVisuals 都
			// 销毁并重建节点（卡顿主因）。状态变化时由 XiaofujiuCardBase.RefreshOverlay
			// 先 Clear 再 Add 处理，所以这里不需要比较纹理。
			if (HasOverlay(container)) return;

			var tex = GetOverlayTexture(baseCard);

			Control overlay;
			if (tex != null)
			{
				var img = new TextureRect();
				img.Name = "FufuOverlay";
				img.MouseFilter = Control.MouseFilterEnum.Ignore;
				img.Texture = tex;
				img.Modulate = Colors.White;
				img.Size = new Vector2(IconSize, IconSize);
				overlay = img;
			}
			else
			{
				overlay = CreateLabel(baseCard);
			}

			overlay.Position = new Vector2(-100, -300);
			overlay.ZIndex = 0;

			container.AddChild(overlay);
		}

		private static Texture2D? GetOverlayTexture(XiaofujiuCardBase baseCard)
		{
			if (baseCard.IsFufuYellow)
			{
				return _yellowTexture ??= LoadTexture("res://images/ui/fufu_yellow_overlay.png");
			}
			if (baseCard.IsFufuBlack)
			{
				return _blackTexture ??= LoadTexture("res://images/ui/fufu_black_overlay.png");
			}
			return _whiteTexture ??= LoadTexture("res://images/ui/fufu_white_overlay.png");
		}

		private static Texture2D? LoadTexture(string path)
		{
			return ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
		}

		private static bool HasOverlay(Node container)
		{
			foreach (var child in container.GetChildren())
			{
				if (child.Name == "FufuOverlay")
				{
					return true;
				}
			}
			return false;
		}

		private static Label CreateLabel(XiaofujiuCardBase baseCard)
		{
			var label = new Label();
			label.Name = "FufuOverlay";
			label.Text = baseCard.IsFufuYellow ? "夫黄" : baseCard.IsFufuBlack ? "夫黑" : "夫白";
			label.Modulate = new Color(1, 1, 1, 0.9f);
			label.Size = new Vector2(48, 24);
			label.Position = new Vector2(100, -140);
			return label;
		}
	}
}
