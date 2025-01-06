using Godot;
using System;

public partial class MapAttackObject : Node2D
{
    [Export] public AnimationPlayer animation;
    [Export] public Sprite2D image;
    [Export] public Area2D area;
    [Export] public Vector2 velocity;
    [Export] public bool isNear = false;
    public double nowTime = 0;
    [Export] public double dieTime = 20;

    private ItemBaseResource item;
    private bool isTouch = false;

    public IAttack attacker
    {
        get 
        { 
            return item as IAttack; 
        } 
    }

    public const string clip_idle = "idle";

    public void SetData(ItemBaseResource item) 
    {
        this.item = item;
    }

    public override void _Ready()
    {
        base._Ready();
        animation.Play(clip_idle);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        nowTime += delta;
        CheckDie();
        Move(delta);
    }

    public void Move(double delta)
    {
        GlobalPosition = new Vector2(GlobalPosition.X + velocity.X * (float)delta, GlobalPosition.Y + velocity.Y * (float)delta);
    }

    public void CheckDie() 
    {
        if(nowTime >= dieTime) {
            QueueFree();        
        }
    }

    public async void OnTouchMonster(Area2D area) 
    {
        if(!isTouch && area.Owner is MonsterObject monsterObj && monsterObj.data.nowHp > 0) 
        {
            isTouch = true;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            WaitToDamage(monsterObj);
        }
    }

    private void WaitToDamage(MonsterObject monsterObj) 
    {
        monsterObj.data.Damage(attacker.attackPoint);
        GameManager.instance.mapManager.PlayFX(attacker.fx, monsterObj.GlobalPosition);
        GameManager.instance.cameraManager.ShakeCamera(3);
        if (attacker.sound != null && attacker.sound != "")
        {
            GameManager.instance.soundManager.PlaySound(attacker.sound);
        }

        if (!isNear) 
        {
            QueueFree();
        }
    }
}
