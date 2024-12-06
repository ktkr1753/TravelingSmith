using Godot;
using System;

public partial class MonsterObject : Node2D
{
	[Export] public Sprite2D mainImage;
    [Export] public AnimationPlayer anim;

    public MonsterResource data;

    public const double closeDistance = 50;


    public void SetData(MonsterResource data) 
    {
        this.data = data;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        BehaviorMachine(delta);
    }

    public void BehaviorMachine(double delta) 
    {
        if(data == null) 
        {
            return;
        }

        if (IsCloseTarget()) 
        {
            Attack(delta);
        }
        else 
        {
            Move(delta);
        }
    }




    public bool IsCloseTarget() 
    {
        bool result = false;

        if(GameManager.instance.mapManager.nowMap?.targetPoint != null)
        {
            if(GlobalPosition.DistanceTo(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition) < closeDistance) 
            {
                result = true;
            }
        }

        return result;
    }

    public void Attack(double delta) 
    {
        double addTime = delta * GameManager.instance.battleManager.gameSpeed;
        if (data.attackNowTime + addTime > data.attackNeedTime) 
        {
            data.attackNowTime = (data.attackNowTime + addTime) % data.attackNeedTime;

            GameManager.instance.battleManager.Damage(data.attackPoint);
        }
        else 
        {
            data.attackNowTime = data.attackNowTime + addTime;
        }
    }


    public void Move(double delta) 
    {
        double addTime = delta * GameManager.instance.battleManager.gameSpeed;
        Vector2 moveNormal = (GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition - GlobalPosition).Normalized();

        GlobalPosition = GlobalPosition + (moveNormal * (float)(data.moveSpeed * addTime));
    }


    public void Destroy()
    {
        QueueFree();
    }
}
