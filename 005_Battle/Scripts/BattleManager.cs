using Godot;
using System;

public partial class BattleManager : Node
{
	[Export] public int maxHP;
	[Export] public int nowHP;

    private int _nowExp = 0;
    [Export] public int nowExp 
    {
        get { return _nowExp; }
        set 
        {
            if(_nowExp != value) 
            {
                int preState = _nowExp;
                _nowExp = value;
                OnExpChange(preState, _nowExp);
                onExpChange?.Invoke(preState, _nowExp);
            }
        }
    }

    private void OnExpChange(int preExp, int nextExp) 
    {
        if(preExp <= nextExp) 
        {
            for(int level = nowLevel; level < GameManager.instance.expConfig.expAllIntervals.Count; level++) 
            {
                int needExp = GameManager.instance.expConfig.expAllIntervals[level];

                if (needExp > nextExp) 
                {
                    nowLevel = level;
                    break;
                }
            }
        }
        else 
        {
            bool isFind = false;
            for (int level = nowLevel; level >= 0; level--)
            {
                int needExp = GameManager.instance.expConfig.expAllIntervals[level];
                if (needExp < nextExp)
                {
                    isFind = true;
                    nowLevel = level + 1;
                    break;
                }
            }

            if (!isFind) 
            {
                nowLevel = 0;
            }
        }
    }

    public event Action<int, int> onExpChange;


    private int _nowLevel = 0;
    [Export] public int nowLevel 
    {
        get { return _nowLevel; }
        set 
        {
            if (_nowLevel != value) 
            {
                int preState = _nowLevel;
                _nowLevel = value;
                OnLevelChange(preState, _nowLevel);
                onLevelChange?.Invoke(preState, _nowLevel);
            }
        }
    }

    private void OnLevelChange(int preLevel, int nextLevel) 
    {

    }

    public event Action<int, int> onLevelChange;

    public int maxLevel 
    {
        get 
        {
            return GameManager.instance.expConfig.expIntervals.Count;
        }
    }


    public bool isGameOver = false;

	public event Action<int> onHPChange;

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

    public void Repair(int repairPoint) 
    {
        if (repairPoint > 0) 
        {
            nowHP = Math.Min(maxHP, nowHP + repairPoint);

            onHPChange?.Invoke(nowHP);
        }
    }
}
