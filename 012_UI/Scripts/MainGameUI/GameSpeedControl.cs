using Godot;
using System;

public partial class GameSpeedControl : Control
{
    [Export] TextureButton pauseButton;
    [Export] TextureButton playButton;
    [Export] TextureButton fastForwardButton;

    public override void _Ready()
    {
        base._Ready();

        GameManager.instance.localSetting.onGameSpeedSettingChange += CheckGameSpeedChangeButtons;

        CheckGameSpeedChangeButtons(GameManager.instance.localSetting.gameSpeedSetting);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.localSetting.onGameSpeedSettingChange -= CheckGameSpeedChangeButtons;
    }

    public void CheckGameSpeedChangeButtons(double gameSpeed) 
    {
        if (gameSpeed == 0)
        {
            playButton.ButtonPressed = false;
            fastForwardButton.ButtonPressed = false;
            pauseButton.ButtonPressed = true;
        }
        else if (gameSpeed == 2)
        {
            pauseButton.ButtonPressed = false;
            playButton.ButtonPressed = false;
            fastForwardButton.ButtonPressed = true;
        }
        else
        {
            pauseButton.ButtonPressed = false;
            fastForwardButton.ButtonPressed = false;
            playButton.ButtonPressed = true;
        }
    }

    public void OnPauseClick() 
	{
        GameManager.instance.localSetting.gameSpeedSetting = 0;
    }

	public void OnPlayClick() 
	{
		GameManager.instance.localSetting.gameSpeedSetting = 1;
	}

	public void OnFastForwardClick() 
	{
        GameManager.instance.localSetting.gameSpeedSetting = 2;
    }
}
