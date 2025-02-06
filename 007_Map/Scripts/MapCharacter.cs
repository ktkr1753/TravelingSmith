using Godot;
using System;

public partial class MapCharacter : Node2D
{
	[Export] private AnimationPlayer animation;

	public const string clip_front_idle = "front_idle";
    public const string clip_front_walk = "front_walk";
    public const string clip_back_idle = "back_idle";
    public const string clip_back_walk = "back_walk";

    public const float moveSpeed = 32.0f;

    Tween moveTween;

    public override void _EnterTree()
    {
        base._EnterTree();

        GameManager.instance.onNeedPauseChange += OnGamePause;
        GameManager.instance.localSetting.onGameSpeedSettingChange += OnGameSpeedChange;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.onNeedPauseChange -= OnGamePause;
        GameManager.instance.localSetting.onGameSpeedSettingChange -= OnGameSpeedChange;
    }

    public void Init(Vector2 startPoint) 
    {
        GlobalPosition = startPoint;
        animation.Play(clip_front_idle);
    }

    public void PlayIn(Vector2 startPoint, Vector2 endPoint) 
    {
        GlobalPosition = startPoint;
        animation.Play(clip_front_walk);

        float dis = startPoint.DistanceTo(endPoint);

        float duration = dis / moveSpeed;

        moveTween = GetTree().CreateTween();
        moveTween.TweenProperty(this, "global_position", endPoint, duration);
        moveTween.TweenCallback(Callable.From(() =>
        {
            moveTween = null;
            animation.Play(clip_front_idle);
        }));
    }

    public void PlayOut(Vector2 startPoint, Vector2 endPoint) 
    {
        GlobalPosition = startPoint;
        animation.Play(clip_back_walk);

        float dis = startPoint.DistanceTo(endPoint);

        float duration = dis / moveSpeed;

        moveTween = GetTree().CreateTween();
        moveTween.TweenProperty(this, "global_position", endPoint, duration);
        moveTween.TweenCallback(Callable.From(() =>
        {
            moveTween = null;
            animation.Play(clip_back_idle);
        }));
    }

    public void OnGamePause(int needPause)
    {
        ResetAnimationSpeed();
    }

    public void OnGameSpeedChange(double gameSpeed)
    {
        ResetAnimationSpeed();
    }

    private void ResetAnimationSpeed()
    {
        if(moveTween != null) 
        {
            moveTween.SetSpeedScale((float)GameManager.instance.gameSpeed);
        }
        animation.SpeedScale = (float)GameManager.instance.gameSpeed;
    }
}
