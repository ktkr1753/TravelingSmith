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
    [Export] public CameraManager cameraManager;

    public RandomManager randomManager = new RandomManager();

    //config
    [Export] public ItemConfigResource itemConfig;
    [Export] public AreaConfigResource areaConfig;
    [Export] public MonsterConfigResource monsterConfig;
    [Export] public ExpConfigResource expConfig;
    [Export] public MapAttackObjectConfigResource mapAttackConfig;
    [Export] public UnlockRecipeResource unlockRecipe;

    [Export] public LocalSettingResource localSetting;
    [Export] public UICommonSettingResource uiCommonSetting;

    private HashSet<int> _isNeedPause = new HashSet<int>();
    public Action AddNeedPause() 
    {

        int rndCode = randomManager.GetRange(RandomType.Other, int.MinValue, int.MaxValue);
        _isNeedPause.Add(rndCode);
        //Debug.Print($"AddNeedPause _isNeedPause count:{_isNeedPause.Count}");
        onNeedPauseChange?.Invoke(_isNeedPause.Count);
        Action finishCallback = () =>
        {
            bool isSuccess = RemoveNeedPause(rndCode);
            if (isSuccess)
            {
                onNeedPauseChange?.Invoke(_isNeedPause.Count);
            }
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

    public event Action<int> onNeedPauseChange;

    private bool RemoveNeedPause(int code) 
    {
        bool isSuccess = false;
        if (_isNeedPause.Contains(code))
        {
            _isNeedPause.Remove(code);
            isSuccess = true;
            //Debug.Print($"AddNeedPause _isNeedPause count:{_isNeedPause.Count}");
        }
        else 
        {
            Debug.PrintWarn($"RemoveNeedPause 不存在code:{code}");
        }

        return isSuccess;
    }


    public double gameSpeed
    {
        get
        {
            double result = 1;
            if (isNeedPause)
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

    public override void _EnterTree()
    {
        base._EnterTree();
        if (instance == null)
        {
            instance = this;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        soundManager.Init();
        uiManager.Init();
        itemManager.Init();
        battleManager.Init();
        mapManager.Init();
        cameraManager.Init();
        InitCamera();

        uiManager.OpenUI(UIIndex.MainGameUI);
        uiManager.OpenUI(UIIndex.MapElementUI);
        uiManager.OpenUI(UIIndex.BattleInfoUI);
        //uiManager.OpenUI(UIIndex.ShopUI);
        soundManager.PlayBGM(SoundEnum.bgm_5);
    }

    private void InitCamera() 
    {
        Rect2 viewportRect = GetViewportRect();
        cameraManager.camera.follow = mapManager.nowMap.main;
    }

    public async Task<Variant[]> Wait(float seconds)
    {
        //Debug.Print("Before Wait");
        var result = await ToSignal(GetTree().CreateTimer(seconds), "timeout");
        //Debug.Print("After Wait");
        return result;
    }
}
