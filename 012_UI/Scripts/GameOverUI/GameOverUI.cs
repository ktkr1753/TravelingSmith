using Godot;
using System;

public partial class GameOverUI : UIBase 
{
	private Action gameOverPause;


    public override void Init()
    {
        base.Init();

        if (parameters[0] is Action finishPause) 
        {
            gameOverPause = finishPause;
        }
    }

    public void OnRestartClick() 
    {
        gameOverPause?.Invoke();
    }
}
