using Godot;
using System;

public partial class GameManager : Node2D
{
    private static GameManager _instance;
    public static GameManager instance { get { return _instance; } private set { _instance = value; } }

    //管理器
    [Export] public UIManager uiManager;
    [Export] public SoundManager soundManager;
    [Export] public ItemManager itemManager;

    //config
    [Export] public ItemConfigResource itemConfig;

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


        uiManager.OpenUI(UIIndex.MainGameUI);
	}
}
