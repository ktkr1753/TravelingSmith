using Godot;
using System;

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

    public double gameSpeed
    {
        get
        {
            double result = 1;
            if (battleManager.isGameOver || isChoosePause)
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
        uiManager.OpenUI(UIIndex.ShopUI);
    }
}
