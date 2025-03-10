using Godot;
using System;

public partial class BattleManager : Node
{
	[Export] public int maxHP;
    private int _nowHP;
	[Export] public int nowHP 
    {
        get { return _nowHP; }
        set 
        { 
            if (_nowHP != value) 
            {
                _nowHP = value;
                if (_nowHP == 0)
                {
                    isGameOver = true;
                    Action finishPause = GameManager.instance.AddNeedPause();
                    GameManager.instance.uiManager.OpenUI(UIIndex.GameOverUI, new System.Collections.Generic.List<object> { finishPause });
                }
            }
        }
    }
    public event Action<int, int, HPChangeType> onHPChange;

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

    public bool _isShellReady = false;
    public bool isShellReady 
    {
        get { return _isShellReady; }
        set 
        {
            if(_isShellReady != value) 
            {
                _isShellReady = value;
                OnIsShellReadyChange(_isShellReady);
                onIsShellReadyChange?.Invoke(_isShellReady);
            }
        }
    }
    private void OnIsShellReadyChange(bool nowIsReady)
    {

    }

    public event Action<bool> onIsShellReadyChange;

    public double nowShellTime = 0;

    public const double shellNeedTime = 20;


    public void Init() 
	{
		isGameOver = false;
        nowHP = maxHP;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdateShellTime(delta);
    }

    private void UpdateShellTime(double delta) 
    {
        if (isShellReady || !GameManager.instance.itemManager.featureContents.Contains(FeatureContentIndex.Shell)) 
        {
            return;
        }

        double addTime = delta * GameManager.instance.gameSpeed;
        if (addTime == 0)
        {
            return;
        }

        if (nowShellTime + addTime > shellNeedTime) 
        {
            isShellReady = true;
            nowShellTime = 0;
        }
        else 
        {
            nowShellTime += addTime;
        }
    }

    public int GetAttackerPoint(IAttack attacker) 
    {
        int attackPoint = attacker.attackPoint;
        if (GameManager.instance.itemManager.featureContents.Contains(FeatureContentIndex.AddAttackDamage)) 
        {
            attackPoint = (int)Math.Ceiling(attackPoint * 1.5);
        }

        if (attacker.durability == 0)
        {
            attackPoint = (int)Math.Floor(attacker.attackPoint / 2.0);
        }

        return attackPoint;
    }

	public void Damage(int damage, HPChangeType type = HPChangeType.Normal) 
	{
		if(damage > 0) 
		{
            if (isShellReady) 
            {
                isShellReady = false;
            }
            else 
            {
                int preHP = nowHP;
			    nowHP = Math.Max(0, nowHP - damage);
                onHPChange?.Invoke(preHP, _nowHP, type);
            }
        }
	}

    public void Repair(int repairPoint, HPChangeType type = HPChangeType.Normal) 
    {
        if (repairPoint > 0) 
        {
            int preHP = nowHP;
            nowHP = Math.Min(maxHP, nowHP + repairPoint);
            onHPChange?.Invoke(preHP, _nowHP, type);
        }
    }
}
