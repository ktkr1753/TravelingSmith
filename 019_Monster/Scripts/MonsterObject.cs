using Godot;
using System;

public partial class MonsterObject : Node2D
{
    [Export] public ProgressBar hpProgressBar;
	[Export] public Sprite2D mainImage;
    [Export] public AnimationPlayer anim;

    public MonsterResource data;

    public event Action<MonsterObject> onDestroy;

    public const double closeDistance = 15;


    public void SetData(MonsterResource data) 
    {
        UnregisterEvent(this.data);
        this.data = data;
        RegisterEvent(this.data);

        SetView();
    }

    private void RegisterEvent(MonsterResource data) 
    {
        if (data != null)
        {
            data.onHPChange += OnHpChange;
            data.onDie += Destroy;
        }
    }
    private void UnregisterEvent(MonsterResource data)
    {
        if(data != null) 
        {
            data.onHPChange -= OnHpChange;
            data.onDie -= Destroy;
        }
    }

    private void SetView() 
    {
        SetHpProgressbar();
    }

    private void SetHpProgressbar() 
    {
        if(data != null) 
        {
            if (data.maxHp == data.nowHp)
            {
                hpProgressBar.Visible = false;
            }
            else 
            {
                hpProgressBar.Visible = true;
                hpProgressBar.MaxValue = data.maxHp;
                hpProgressBar.Value = data.nowHp;
            }
        }
    }

    private void ShowBattleHPInfo(int hpChange) 
    {
        BattleInfoUI battleInfoUI = GameManager.instance.uiManager.GetOpenedUI<BattleInfoUI>(UIIndex.BattleInfoUI);

        if(hpChange < 0) 
        {
            Vector2 viewPos = (GetViewportRect().Size / 2) + (GlobalPosition - GameManager.instance.mapManager.nowMap.camera.GlobalPosition);
            Vector2 showPos = new Vector2(viewPos.X, viewPos.Y + -12);
            battleInfoUI.ShowMinusHPInfo(-hpChange, showPos);
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
            if (!IsTooCloseTarget()) 
            {
                Move(delta);
            }
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

    public bool IsTooCloseTarget() 
    {
        bool result = false;

        if (GameManager.instance.mapManager.nowMap?.targetPoint != null)
        {
            if (GlobalPosition.DistanceTo(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition) < (closeDistance / 2))
            {
                result = true;
            }
        }

        return result;
    }

    public void Attack(double delta) 
    {
        double addTime = delta * GameManager.instance.gameSpeed;
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
        double addTime = delta * GameManager.instance.gameSpeed;
        Vector2 moveNormal = (GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition - GlobalPosition).Normalized();

        GlobalPosition = GlobalPosition + (moveNormal * (float)(data.moveSpeed * addTime));
    }

    private void OnHpChange(int preHP,int nowHp) 
    {
        SetHpProgressbar();
        ShowBattleHPInfo(nowHp - preHP);
    }

    public void Destroy()
    {
        QueueFree();
    }
}
