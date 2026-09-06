using Godot;

public partial class IdlePreview : Node2D
{
	public override void _Ready()
	{
		var anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (anim == null)
		{
			GD.PrintErr("找不到 AnimatedSprite2D 节点");
			return;
		}
		if (anim.SpriteFrames == null)
		{
			GD.PrintErr("SpriteFrames 为空！检查 idle_sheet.tres 是否正确加载");
			return;
		}
		var names = anim.SpriteFrames.GetAnimationNames();
		GD.Print("动画列表: " + string.Join(", ", names));
		anim.Play("idle");
	}
}
