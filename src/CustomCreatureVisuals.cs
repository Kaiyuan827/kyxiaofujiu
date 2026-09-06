using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

public partial class CustomCreatureVisuals : NCreatureVisuals
{
	private AnimatedSprite2D? _anim;

	// 自动播放待机动画（Visuals/AnimatedSprite2D 引用 xiaofujiu_anims.tres）
	public override void _Ready()
	{
		base._Ready();
		_anim = GetNodeOrNull<AnimatedSprite2D>("Visuals/AnimatedSprite2D");
		if (_anim != null && _anim.SpriteFrames != null)
		{
			_anim.AnimationFinished += OnAnimationFinished;
			_anim.Play("idle");
		}
	}

	private void OnAnimationFinished()
	{
		if (_anim == null) return;
		// 非循环战斗动画播完回待机；死亡动画保持最后一帧
		var current = _anim.Animation;
		if (current != "idle" && current != "death")
		{
			_anim.Play("idle");
		}
	}

	/// <summary>
	/// 供 Harmony 补丁调用：播放战斗动画（attack/skill/power/hurt/death/idle）
	/// </summary>
	public void PlayAction(string animName)
	{
		if (_anim == null || _anim.SpriteFrames == null) return;
		if (_anim.SpriteFrames.HasAnimation(animName))
		{
			_anim.Play(animName);
		}
	}
}
