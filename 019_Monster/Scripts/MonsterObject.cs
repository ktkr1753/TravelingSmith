using Godot;
using System;

public partial class MonsterObject : Node2D
{
    public enum AnimState 
    {
        None = 0,
        Idle = 1,
        Move = 2,
        Die = 3,
    }

    [Export] public ProgressBar hpProgressBar;
    [Export] public ProgressBar hpBackProgressBar;
	[Export] public Sprite2D mainImage;
    [Export] public AnimationPlayer anim;
    [Export] public AnimationPlayer exclamationAnim;

    public MonsterResource data;
    private bool _isFind = false;
    public bool isFind 
    {
        get { return _isFind; }
        set 
        {
            if(_isFind != value) 
            {
                _isFind = value;
                if (_isFind) 
                {
                    PlayExclamation();
                }
            }
        }
    }

    private AnimState _nowAnimState = AnimState.None;
    public AnimState nowAnimState 
    {
        get { return _nowAnimState; }
        set
        {
            if (_nowAnimState != value)
            {
                AnimState preState = _nowAnimState;
                _nowAnimState = value;
                OnAnimStateChange(preState, _nowAnimState);
            }
        }
    }

    private void OnAnimStateChange(AnimState preState, AnimState nextState) 
    {
        switch (nextState) 
        {
            case AnimState.Idle:
                anim.Play(clip_idle);
                break;
            case AnimState.Move:
                anim.Play(clip_move);
                break;
            case AnimState.Die:
                anim.Play(clip_die);
                break;
        }
    }


    private bool isDie = false;

    private Tween hpTween = null;

    public event Action<MonsterObject> onDie;
    public event Action<MonsterObject> onDestroy;

    public const double findDistance = 200;
    public const double closeDistance = 3;

    public const string material_rate = "rate";
    public const string material_finalColor = "finalColor";

    public const string clip_idle = "idle";
    public const string clip_move = "move";
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

            GameManager.instance.onNeedPauseChange += OnGamePauseChange;
            GameManager.instance.localSetting.onGameSpeedSettingChange += OnGameSpeedChange;
        }
    }
    private void UnregisterEvent(MonsterResource data)
    {
        if(data != null) 
        {
            data.onHPChange -= OnHpChange;
            data.onDie -= OnDie;

            GameManager.instance.onNeedPauseChange -= OnGamePauseChange;
            GameManager.instance.localSetting.onGameSpeedSettingChange -= OnGameSpeedChange;
        }
    }

    private void SetView() 
    {
        InitHpProgressbar();
        nowAnimState = AnimState.Idle;
    }

    private void InitHpProgressbar() 
    {
        if (data != null)
        {
            hpProgressBar.MaxValue = data.maxHp;
            hpProgressBar.Value = data.nowHp;
            hpBackProgressBar.MaxValue = data.maxHp;
            hpBackProgressBar.Value = data.nowHp;

            if (data.maxHp == data.nowHp || data.nowHp == 0)
            {
                hpProgressBar.Visible = false;
                hpBackProgressBar.Visible = false;
            }
            else
            {
                hpProgressBar.Visible = true;
                hpBackProgressBar.Visible = true;
            }
        }
    }

    private void SetHpProgressbar() 
    {
        if(data != null) 
        {
            if (data.maxHp == data.nowHp || data.nowHp == 0)
            {
                hpProgressBar.Visible = false;
                hpBackProgressBar.Visible = false;
            }
            else 
            {

                hpProgressBar.Visible = true;
                hpProgressBar.Value = data.nowHp;
                hpBackProgressBar.Visible = true;

                int targetHP = data.nowHp;
                if(hpTween != null) 
                {
                    hpTween.Stop();
                    hpTween = null;
                }
                //Debug.Print($"SetHpProgressbar data.maxHp:{data.maxHp},data.nowHp:{data.nowHp}, hpBackProgressBar.Value:{hpBackProgressBar.Value}");

                hpTween = CreateTween();
                hpTween.SetEase(Tween.EaseType.Out);
                hpTween.TweenProperty(hpBackProgressBar, "value", targetHP, 1f);
                hpTween.TweenCallback(Callable.From(() =>
                {
                    hpTween.Stop();
                    hpTween = null;
                }));
            }
        }
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
            else 
            {
                nowAnimState = AnimState.Idle;
            }
            Attack(delta);
        }
        else if(isFind)
        {
            Move(delta);
        }
        else 
        {
            if (GameManager.instance.mapManager.nowMap?.main != null)
            {
                if (Math.Abs(GameManager.instance.mapManager.nowMap.main.GlobalPosition.X - GlobalPosition.X) < findDistance)
                {
                    isFind = true;
                }
            }
        }
    }

    public bool IsCloseTarget() 
    {
        bool result = false;

        if(GameManager.instance.mapManager.nowMap?.main != null)
        {
            if (Math.Abs(GameManager.instance.mapManager.nowMap.main.GlobalPosition.X - GlobalPosition.X) < closeDistance)
            {
                result = true;
            }
        }

        return result;
    }

    public bool IsTooCloseTarget() 
    {
        bool result = false;

        if (GameManager.instance.mapManager.nowMap?.main != null)
        {
            //if (GlobalPosition.DistanceTo(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition) < (closeDistance / 2))
            if (Math.Abs(GameManager.instance.mapManager.nowMap.main.GlobalPosition.X - GlobalPosition.X) < (closeDistance / 2))
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
            Vector2 targetPos = GameManager.instance.mapManager.nowMap.main.Position;
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

        nowAnimState = AnimState.Move;
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

    private void ResetAnimationSpeed() 
    {
        anim.SpeedScale = (float)GameManager.instance.gameSpeed;
        exclamationAnim.SpeedScale = (float)GameManager.instance.gameSpeed;
    }

    private void OnHpChange(int preHP,int nowHp, HPChangeType type) 
    {
        SetHpProgressbar();
        ShowBattleHPInfo(nowHp - preHP, type);

        if(nowHp < preHP) 
        {
            HurtShine();
            isFind = true;
        }
    }

    public void OnDie()
    {
        //QueueFree();
        isDie = true;
        nowAnimState = AnimState.Die;
        onDie?.Invoke(this);
    }

    private void OnGamePauseChange(int needPause) 
    {
        ResetAnimationSpeed();
    }

    private void OnGameSpeedChange(double gameSpeed) 
    {
        ResetAnimationSpeed();
    }
}
