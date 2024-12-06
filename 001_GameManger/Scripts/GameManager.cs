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
    [Export] public MonsterConfigResource monsterConfig;

    [Export] public LocalSettingResource localSetting;
    [Export] public UICommonSettingResource uiCommonSetting;


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
	}
}
