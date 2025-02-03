using Godot;
using System;

public partial class MonsterObject : Node2D
{
    [Export] public ProgressBar hpProgressBar;
	[Export] public Sprite2D mainImage;
    [Export] public AnimationPlayer anim;
    [Export] public AnimationPlayer exclamationAnim;

    public MonsterResource data;
    private bool isFind = false;
    private bool isDie = false;

    public event Action<MonsterObject> onDie;
    public event Action<MonsterObject> onDestroy;

    public const double findDistance = 200;
    public const double closeDistance = 15;

    public const string material_rate = "rate";
    public const string material_finalColor = "finalColor";

    public const string clip_idle = "idle";
    public const string clip_die = "die";

    public void SetData(MonsterResource data) 
    {
        UnregisterEvent(this.data);
        this.data = data;
        isDie = false;
        RegisterEvent(this.data);

        SetView();
    }

    private void RegisterEvent(MonsterResource data) 
    {
        if (data != null)
        {
            data.onHPChange += OnHpChange;
            data.onDie += OnDie;
        }
    }
    private void UnregisterEvent(MonsterResource data)
    {
        if(data != null) 
        {
            data.onHPChange -= OnHpChange;
            data.onDie -= OnDie;
        }
    }

    private void SetView() 
    {
        SetHpProgressbar();
        SetIdle();
    }

    private void SetHpProgressbar() 
    {
        if(data != null) 
        {
            if (data.maxHp == data.nowHp || data.nowHp == 0)
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

    private void SetIdle() 
    {
        anim.Play(clip_idle);
    }

    private void ShowBattleHPInfo(int hpChange, HPChangeType type) 
    {
        BattleInfoUI battleInfoUI = GameManager.instance.uiManager.GetOpenedUI<BattleInfoUI>(UIIndex.BattleInfoUI);

        if(hpChange <= 0) 
        {
            Vector2 viewPos = (GetViewportRect().Size / 2) + (GlobalPosition - GameManager.instance.cameraManager.camera.GlobalPosition);
            Vector2 showPos = new Vector2(viewPos.X, viewPos.Y + -12);
            battleInfoUI.ShowMinusHPInfo(-hpChange, showPos, type);
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
        if(data == null || isDie) 
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
        else if(isFind)
        {
            Move(delta);
        }
        else 
        {
            if (GameManager.instance.mapManager.nowMap?.targetPoint != null)
            {
                if (Math.Abs(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition.X - GlobalPosition.X) < findDistance)
                {
                    isFind = true;
                    PlayExclamation();
                }
            }
        }
    }

    public bool IsCloseTarget() 
    {
        bool result = false;

        if(GameManager.instance.mapManager.nowMap?.targetPoint != null)
        {
            if (Math.Abs(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition.X - GlobalPosition.X) < closeDistance)
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
            //if (GlobalPosition.DistanceTo(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition) < (closeDistance / 2))
            if (Math.Abs(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition.X - GlobalPosition.X) < (closeDistance / 2))
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

            if(data.sound != null && data.sound != "") 
            {
                GameManager.instance.soundManager.PlaySound(data.sound);
            }

        }
        else 
        {
            data.attackNowTime = data.attackNowTime + addTime;
        }
    }


    public void Move(double delta) 
    {
        double addTime = delta * GameManager.instance.gameSpeed;
        //Vector2 moveNormal = (GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition - GlobalPosition).Normalized();
        Vector2 moveNormal = new Vector2(-1, 0);

        GlobalPosition = GlobalPosition + (moveNormal * (float)(data.moveSpeed * addTime));
    }

    private async void HurtShine() 
    {
        ShaderMaterial shineMaterial = mainImage.Material as ShaderMaterial;

        shineMaterial.SetShaderParameter(material_rate, 1.0);
        await GameManager.instance.Wait(0.2f);
        if(shineMaterial == null) 
        {
            return;
        }
        shineMaterial.SetShaderParameter(material_rate, 0.0);
    }

    private void PlayExclamation() 
    {
        exclamationAnim.Play(clip_idle);
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_beat_1);
    }

    private void OnHpChange(int preHP,int nowHp, HPChangeType type) 
    {
        SetHpProgressbar();
        ShowBattleHPInfo(nowHp - preHP, type);

        if(nowHp < preHP) 
        {
            HurtShine();
        }
    }

    public void OnDie()
    {
        //QueueFree();
        isDie = true;
        anim.Play(clip_die);
        onDie?.Invoke(this);
    }
}
