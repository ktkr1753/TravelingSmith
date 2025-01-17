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

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "global_position", endPoint, duration);
        tween.TweenCallback(Callable.From(() =>
        {
            animation.Play(clip_front_idle);
        }));
    }

    public void PlayOut(Vector2 startPoint, Vector2 endPoint) 
    {
        GlobalPosition = startPoint;
        animation.Play(clip_back_walk);

        float dis = startPoint.DistanceTo(endPoint);

        float duration = dis / moveSpeed;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "global_position", endPoint, duration);
        tween.TweenCallback(Callable.From(() =>
        {
            animation.Play(clip_back_idle);
        }));
    }
}
