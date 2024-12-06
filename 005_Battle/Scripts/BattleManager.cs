using Godot;
using System;

public partial class BattleManager : Node
{
	[Export] public int maxHP;
	[Export] public int nowHP;

	public bool isGameOver = false;

	public event Action<int> onHPChange;

    public double gameSpeed
    {
        get
        {
            double result = 1;
            if (isGameOver)
            {
                result = 0;
            }
            else
            {
                result = GameManager.instance.localSetting.gameSpeedSetting;
            }
            return result;
        }
    }

    public void Init() 
	{
		isGameOver = false;
        nowHP = maxHP;
    }

	public void Damage(int damage) 
	{
		if(damage > 0) 
		{
			nowHP = Math.Max(0, nowHP - damage);
			//後續處理
			onHPChange?.Invoke(nowHP);

			if(nowHP == 0) 
			{
				isGameOver = true;
				GameManager.instance.uiManager.OpenUI(UIIndex.GameOverUI);
			}
        }
	}
}
