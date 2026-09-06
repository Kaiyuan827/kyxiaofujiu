using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;
using kyxiaofujiu.core;

namespace kyxiaofujiu.utils
{
	/// <summary>
	/// 工资花费图标 — 在需要花费工资的卡牌上，把工资图标显示在卡牌能量图标正下方
	/// （参考储君辉星图标的位置：card.tscn 中 StarIcon 位于 EnergyIcon 正下方）
	/// </summary>
	public static class SalaryOverlayHelper
	{
		private const string NodeName = "SalaryOverlay";
		private const string LabelName = "SalaryOverlayLabel";

		// 缓存工资图标纹理，避免每次 UpdateVisuals 都重新 ResourceLoader.Load（性能）
		private static Texture2D? _salaryTexture;

		// 工资图标位置/大小：位于夫白图标（FufuOverlay: Position(-100,-300), 96x96）的左下方。
		// 夫白图标范围 x[-100,-4] y[-300,-204]；工资图标 58x58 放其左下方：
		//   x < -100（夫白左边），y > -204（夫白底部下方）
		//   → Position(-162,-154)，范围 x[-162,-104] y[-154,-96]，与夫白不重叠
		private static readonly Vector2 IconPos = new Vector2(-162f, -154f);
		private static readonly Vector2 IconSize = new Vector2(58f, 58f);

		public static void Clear(NCard cardNode)
		{
			if (cardNode == null) return;
			var container = cardNode.OverlayContainer;
			if (container == null) return;

			// 立即移除（QueueFree 延迟到下一帧，同帧多次 Add 会残留叠加）
			foreach (var child in container.GetChildren())
			{
				if (child.Name == NodeName || child.Name == LabelName)
				{
					container.RemoveChild(child);
					child.QueueFree();
				}
			}
		}

		public static void Add(NCard cardNode, XiaofujiuCardBase baseCard)
		{
			if (cardNode == null || baseCard == null) return;
			if (!baseCard.HasSalaryCost) return;

			var container = cardNode.OverlayContainer;
			if (container == null) return;
			if (HasOverlay(container)) return;

			var tex = _salaryTexture ??= LoadTexture("res://images/ui/salary.png");
			if (tex == null) return;

			// 工资图标（能量正下方）——ZIndex 与夫黑夫白夫黄图标一致(0)，避免盖住其他卡牌元素
			var icon = new TextureRect
			{
				Name = NodeName,
				MouseFilter = Control.MouseFilterEnum.Ignore,
				Texture = tex,
				Position = IconPos,
				Size = IconSize,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				ZIndex = 0
			};
			container.AddChild(icon);

			// 工资花费数字（图标中央，参考 StarLabel 样式）——ZIndex 与图标相同(0)，靠节点顺序盖在图标上
			var label = new Label
			{
				Name = LabelName,
				Text = baseCard.SalaryCost.ToString(),
				Position = IconPos,
				Size = IconSize,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = Control.MouseFilterEnum.Ignore,
				ZIndex = 0
			};
			label.AddThemeFontSizeOverride("font_size", 22);
			label.AddThemeColorOverride("font_color", new Color(1f, 0.9647f, 0.8863f));
			label.AddThemeColorOverride("font_outline_color", new Color(0.1008f, 0.3668f, 0.42f));
			label.AddThemeConstantOverride("outline_size", 12);
			label.AddThemeConstantOverride("shadow_outline_size", 12);
			container.AddChild(label);

			UpdateAffordability(cardNode, baseCard);
		}

		/// <summary>
		/// 根据工资是否足够更新图标颜色：不足时图标/数字变红
		/// </summary>
		public static void UpdateAffordability(NCard cardNode, XiaofujiuCardBase baseCard)
		{
			if (cardNode == null || baseCard == null || !baseCard.HasSalaryCost) return;
			// canonical 模型（卡牌大全中的展示卡）没有 Owner，跳过变红判断
			if (baseCard.IsCanonical) return;
			var container = cardNode.OverlayContainer;
			if (container == null) return;

			var icon = FindChild(container, NodeName);
			var label = FindChild(container, LabelName);
			if (icon == null && label == null) return;

			bool canAfford = baseCard.CurrentSalary >= baseCard.SalaryCost;
			var color = canAfford ? Colors.White : new Color(1f, 0.3f, 0.3f);

			if (icon is Control ic)
			{
				ic.Modulate = color;
			}
			if (label is Label lb)
			{
				lb.Modulate = color;
			}
		}

		private static Texture2D? LoadTexture(string path)
		{
			return ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
		}

		private static bool HasOverlay(Node container)
		{
			return FindChild(container, NodeName) != null || FindChild(container, LabelName) != null;
		}

		private static Node? FindChild(Node container, string name)
		{
			foreach (var child in container.GetChildren())
			{
				if (child.Name == name)
				{
					return child;
				}
			}
			return null;
		}
	}
}
