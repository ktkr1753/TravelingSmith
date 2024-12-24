using Godot;
using System;
using System.Threading.Tasks;

public partial class FXObject : Node2D
{
	[Export] Sprite2D image;
    [Export] AnimationPlayer animation;

    public const string clip_FX1 = "FX1";
    public const string clip_FX2 = "FX2";
    public const string clip_FX3 = "FX3";
    public const string clip_FX4 = "FX4";
    public const string clip_FX5 = "FX5";
    public const string clip_FX6 = "FX6";
    public const string clip_FX7 = "FX7";
    public const string clip_FX8 = "FX8";
    public const string clip_FX9 = "FX9";
    public const string clip_FX10 = "FX10";
    public override void _Ready()
    {
        base._Ready();

        GameManager.instance.localSetting.onGameSpeedSettingChange += OnGameSpeedChange;
		OnGameSpeedChange(GameManager.instance.localSetting.gameSpeedSetting);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.localSetting.onGameSpeedSettingChange -= OnGameSpeedChange;
    }

	private void OnGameSpeedChange(double gameSpeed) 
	{
		animation.SpeedScale = (float)gameSpeed;
	}

    public async Task PlayFX(FXEnum fxEnum) 
	{
		bool isFail = false;
		switch (fxEnum) 
		{
			case FXEnum.FX1:
				{
					animation.Play(clip_FX1);
                }
				break;
            case FXEnum.FX2:
                {
                    animation.Play(clip_FX2);
                }
                break;
            case FXEnum.FX3:
                {
                    animation.Play(clip_FX3);
                }
                break;
            case FXEnum.FX4:
                {
                    animation.Play(clip_FX4);
                }
                break;
            case FXEnum.FX5:
                {
                    animation.Play(clip_FX5);
                }
                break;
            case FXEnum.FX6:
                {
                    animation.Play(clip_FX6);
                }
                break;
            case FXEnum.FX7:
                {
                    animation.Play(clip_FX7);
                }
                break;
            case FXEnum.FX8:
                {
                    animation.Play(clip_FX8);
                }
                break;
            case FXEnum.FX9:
                {
                    animation.Play(clip_FX9);
                }
                break;
            case FXEnum.FX10:
                {
                    animation.Play(clip_FX10);
                }
                break;
            default: 
				{
					Debug.PrintErr($"未定義fx:{fxEnum}");
					isFail = true;
				}
				break;
		}

		if (!isFail) 
		{
			await ToSignal(animation, "animation_finished").ToTask();
		}
    }
}
