using Godot;
using System;

[GlobalClass]
public partial class Camera2DEX : Camera2D
{
    public enum FollowType
    {
        Default = 0,
        Horizontal = 1,
        Vertical = 2,
    }

    private FollowType _followType = FollowType.Default;
    [Export] public FollowType followType 
    {
        get { return _followType; }
        set { _followType = value; }
    }
    private Node2D _follow;
    [Export] public Node2D follow 
    {
        get { return _follow; }
        set { _follow = value; }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Follow();
    }

    private void Follow() 
    {
        if(follow != null) 
        {
            switch (followType) 
            {
                case FollowType.Default:
                    {
                        GlobalPosition = follow.GlobalPosition;
                    }
                    break;
                case FollowType.Horizontal:
                    {
                        GlobalPosition = new Vector2(follow.GlobalPosition.X, GlobalPosition.Y);
                    }
                    break;
                case FollowType.Vertical:
                    {
                        GlobalPosition = new Vector2(GlobalPosition.X, follow.GlobalPosition.Y);
                    }
                    break;
            }

        }
    }
}
