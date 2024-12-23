using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class GameManager : Node2D
{
    private static GameManager _instance;
    public static GameManager instance { get { return _instance; } private set { _instance = value; } }

    //管理器
    [Export] public MapManager mapManager;
    [Export] public UIManager uiManager;
    [Export] public SoundManager soundManager;
    [Export] public ItemManager itemManager;
    [Export] public BattleManager battleManager;

    public RandomManager randomManager = new RandomManager();

    //config
    [Export] public ItemConfigResource itemConfig;
    [Export] public AreaConfigResource areaConfig;
    [Export] public MonsterConfigResource monsterConfig;
    [Export] public ExpConfigResource expConfig;

    [Export] public LocalSettingResource localSetting;
    [Export] public UICommonSettingResource uiCommonSetting;

    public bool _isChoosePause = false;
    public bool isChoosePause 
    {
        get { return _isChoosePause; } 
        set 
        {  
            _isChoosePause = value; 
        }
    }

    private HashSet<int> _isNeedPause = new HashSet<int>();
    public Action AddNeedPause() 
    {

        int rndCode = randomManager.GetRange(RandomType.Other, int.MinValue, int.MaxValue);
        _isNeedPause.Add(rndCode);
        //Debug.Print($"AddNeedPause _isNeedPause count:{_isNeedPause.Count}");
        Action finishCallback = () =>
        {
            RemoveNeedPause(rndCode);
        };

        return finishCallback;
    }

    public bool isNeedPause 
    {
        get 
        {
            bool result = false;
            if(_isNeedPause.Count > 0) 
            {
                result = true;
            }
            return result;
        }
    }

    private void RemoveNeedPause(int code) 
    {
        if (_isNeedPause.Contains(code))
        {
            _isNeedPause.Remove(code);
            //Debug.Print($"AddNeedPause _isNeedPause count:{_isNeedPause.Count}");
        }
        else 
        {
            Debug.PrintWarn($"RemoveNeedPause 不存在code:{code}");
        }
    }


    public double gameSpeed
    {
        get
        {
            double result = 1;
            if (battleManager.isGameOver || isNeedPause)
            {
                result = 0;
            }
            else
            {
                result = localSetting.gameSpeedSetting;
            }
            return result;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        if (instance == null)
        {
            instance = this;
        }
        soundManager.Init();
        uiManager.Init();
        itemManager.Init();
        battleManager.Init();


        uiManager.OpenUI(UIIndex.MainGameUI);
        uiManager.OpenUI(UIIndex.MapElementUI);
        uiManager.OpenUI(UIIndex.BattleInfoUI);
        //uiManager.OpenUI(UIIndex.ShopUI);
    }

    public async Task<Variant[]> Wait(float seconds)
    {
        //Debug.Print("Before Wait");
        var result = await ToSignal(GetTree().CreateTimer(seconds), "timeout");
        //Debug.Print("After Wait");
        return result;
    }
}
