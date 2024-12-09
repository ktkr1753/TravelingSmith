using Godot;
using System;

public partial class MonsterObject : Node2D
{
	[Export] public Sprite2D mainImage;
    [Export] public AnimationPlayer anim;

    public MonsterResource data;

    public event Action<MonsterObject> onDestroy;

    public const double closeDistance = 15;


    public void SetData(MonsterResource data) 
    {
        if(this.data != null) 
        {
            data.onDie -= Destroy;
        }

        this.data = data;

        if(data != null) 
        {
            data.onDie += Destroy;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        BehaviorMachine(delta);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        onDestroy?.Invoke(this);
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

            int rndX = GameManager.instance.randomManager.GetRange(RandomType.Other, -5, 5);
            int rndY = GameManager.instance.randomManager.GetRange(RandomType.Other, -5, 5);
            Vector2 targetPos = GameManager.instance.mapManager.nowMap.targetPoint.Position;
            Vector2 position = new Vector2(targetPos.X + rndX, targetPos.Y + rndY);
            GameManager.instance.mapManager.PlayFX(data.attackFX, position);
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
