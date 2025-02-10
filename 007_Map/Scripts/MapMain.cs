using Godot;
using System;

public partial class MapMain : Node2D
{
    public enum CarState 
    {
        None = 0,
        Idle = 1,
        Move = 2,
    }


	[Export] AnimationPlayer mainAnim;
    [Export] AnimationPlayer crystalAnim;
    [Export] public Node2D attackNode;

    private CarState _nowCarState = CarState.Idle;
    public CarState nowCarState 
    {
        get { return _nowCarState; }
        set 
        { 
            if(_nowCarState != value) 
            {
                CarState preState = _nowCarState;
                _nowCarState = value;
                OnCarStateChange(preState, _nowCarState);
            }
        }
    }

    private void OnCarStateChange(CarState preState, CarState nowState) 
    {
        switch (preState) 
        {
            default:
                mainAnim.Stop();
                break;
        }

        switch (nowState) 
        {
            case CarState.Idle:
                mainAnim.Play(clip_idle);
                break;
            case CarState.Move:
                mainAnim.Play(clip_move);
                break;
        }
    }

    public const string clip_idle = "idle";
    public const string clip_move = "move";
    public const string clip_shine = "shine";

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

    public override void _Ready()
    {
        base._Ready();

        crystalAnim.Play(clip_shine);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        UpdateMoveState(delta);
    }


    private void UpdateMoveState(double delta) 
    {
        if(GameManager.instance.itemManager.moveSpeed >= 5) 
        {
            nowCarState = CarState.Move;
        }
        else 
        {
            nowCarState = CarState.Idle;
        }
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
        mainAnim.SpeedScale = (float)GameManager.instance.gameSpeed;
        crystalAnim.SpeedScale = (float)GameManager.instance.gameSpeed;
    }
}
