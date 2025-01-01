using Godot;
using System;
using System.Collections.Generic;

public partial class ShopUI : UIBase
{
    public enum PositionState 
    {
        None = 0,
        In = 1,
        MovingIn = 2,
        Out = 3,
        MovingOut = 4,
    }

    [Export] private AnimationPlayer animation;
    [Export] private Label refreshCostLabel;
    [Export] private Button refreshButton;
    [Export] private NinePatchRect moveButtonImage;
    [Export] private TextureRect arrorImage;
    [Export] private Godot.Collections.Dictionary<PositionState, Texture2D> moveButtonTextures = new Godot.Collections.Dictionary<PositionState, Texture2D>();
    [Export] private Godot.Collections.Dictionary<PositionState, Texture2D> arrorTextures = new Godot.Collections.Dictionary<PositionState, Texture2D>();
    [Export] private ShopItemElementPool itemPool;
    [Export] private Control sellPanel;
    [Export] private Control refreshHeldPanel;
    [Export] private Label refreshHeldCostLabel;

    private PositionState _positionState = PositionState.Out;
    public PositionState positionState 
    {
        get { return _positionState; }
        set 
        {
            if(_positionState != value) 
            {
                _positionState = value;
                OnPositionStateChange(_positionState);
            }
        }
    }

    private void OnPositionStateChange(PositionState nowPositionState) 
    {
        SetMoveButtonImage();
        SetArrowImage();
    }

    private int _nowPickElementIndex = -1;
    public int nowPickElementIndex
    {
        get { return _nowPickElementIndex; }
        set
        {
            if (_nowPickElementIndex != value)
            {
                int preState = _nowPickElementIndex;
                _nowPickElementIndex = value;

                if (preState >= 0 && preState < itemPool.inuses.Count)
                {
                    itemPool.inuses[preState].itemElement.isPicking = false;
                }

                if (_nowPickElementIndex >= 0 && _nowPickElementIndex < itemPool.inuses.Count)
                {
                    SetPickedItemElement(itemPool.inuses[_nowPickElementIndex].item);
                    itemPool.inuses[_nowPickElementIndex].itemElement.isPicking = true;
                }
                else
                {
                    SetPickedItemElement(null);
                }
            }
        }
    }

    private int refreshCount = 0;
    public int refreshCost 
    {
        get 
        {
            int result = 0;
            result = GameManager.instance.mapManager.nowMap.visitedShopIndex + 1 + (int)Math.Ceiling(refreshCount * 1.5);

            return result;
        }
    }

    private int refreshHeldCount = 0;

    public int refreshHeldCost 
    {
        get
        {
            int result = 0;
            result = GameManager.instance.mapManager.nowMap.visitedShopIndex + 1 + (int)Math.Ceiling(refreshHeldCount * 2.0);
            return result;
        }
    }

    public List<ItemBaseResource> sellItems = new List<ItemBaseResource>();
    private Action pauseFinishCallback;

    public const double flyTime = 0.2;

    public const string clip_moveIn = "moveIn";
    public const string clip_moveOut = "moveOut";

    public override void Init()
    {
        base.Init();

        GameManager.instance.itemManager.onMoneyChange += OnMoneyChange;

        pauseFinishCallback = GameManager.instance.AddNeedPause();
        //測試
        TestRandomItem();

        SetView();
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        pauseFinishCallback?.Invoke();
        pauseFinishCallback = null;

        GameManager.instance.itemManager.onMoneyChange -= OnMoneyChange;
    }

    private void TestRandomItem() 
    {
        sellItems.Clear();
        List<ItemIndex> indexs = new List<ItemIndex>();
        foreach (ItemIndex itemIndex in GameManager.instance.itemConfig.config.Keys) 
        {
            if(GameManager.instance.itemConfig.config.TryGetValue(itemIndex, out ItemBaseResource item)) 
            {
                if(item is CommodityResource commodity || !item.isSellable) 
                {
                    continue;
                }
                indexs.Add(itemIndex);
            }
        }

        for(int i = 0; i < 5; i++) 
        {
            int rnd = GameManager.instance.randomManager.GetRange(RandomType.Other, 0, indexs.Count);

            ItemBaseResource item = GameManager.instance.itemConfig.config[indexs[rnd]];
            sellItems.Add(item);
        }
    }

    private void RefrershItem() 
    {
        GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - refreshCost);
        TestRandomItem();
        SetShopItemElements();

        refreshCount++;
        SetRefreshButton();
    }

    private void SetView() 
    {
        SetMoveButtonImage();
        SetArrowImage();
        SetShopItemElements();
        SetRefreshButton();
        SetRefreshHeldCost();
    }

    private void SetMoveButtonImage() 
    {
        if(moveButtonTextures.TryGetValue(positionState, out Texture2D texture)) 
        {
            moveButtonImage.Texture = texture;
        }
        else 
        {
            Debug.PrintErr($"沒有moveButtonTexture, positionState:{positionState}");
        }
    }
    private void SetArrowImage()
    {
        if (arrorTextures.TryGetValue(positionState, out Texture2D texture))
        {
            arrorImage.Texture = texture;
        }
        else
        {
            Debug.PrintErr($"沒有arrorTexture, positionState:{positionState}");
        }
    }

    private void SetShopItemElements() 
    {
        ClearShopItemElements();
        for (int i = 0; i < sellItems.Count; i++) 
        {
            ShopItemElement element = itemPool.GetElement();
            element.SetData(i, sellItems[i]);
            element.itemElement.onMainButtonDown += OnShopItemButtonDown;
            element.itemElement.onMainButtonUp += OnShopItemButtonUp;
        }
    }

    private void ClearShopItemElements() 
    {
        for(int i = 0; i < itemPool.inuses.Count; i++) 
        {
            ClearShopItemElementData(itemPool.inuses[i]);
        }
        itemPool.ReturnAllElement();
    }

    private void ClearShopItemElementData(ShopItemElement element) 
    {
        element.SetData(-1, null);
        element.itemElement.onMainButtonDown -= OnShopItemButtonDown;
        element.itemElement.onMainButtonUp -= OnShopItemButtonUp;
    }

    public bool IsInSellRect(Vector2 globalPosition) 
    {
        bool result = false;
        Rect2 sellRect = new Rect2(sellPanel.GlobalPosition, sellPanel.Size);
        if (sellRect.HasPoint(globalPosition)) 
        {
            result = true;
        }

        return  result;
    }

    public bool IsInRefreshRect(Vector2 globalPosition) 
    {
        bool result = false;
        Rect2 refreshRect = new Rect2(refreshHeldPanel.GlobalPosition, sellPanel.Size);
        if (refreshRect.HasPoint(globalPosition))
        {
            result = true;
        }

        return result;
    }

    private void OnShopItemButtonDown(int index) 
    {
        //Debug.Print($"OnShopItemButtonDown index:{index}");
        int inuseOrder = -1;
        for(int i = 0; i < itemPool.inuses.Count; i++) 
        {
            if (itemPool.inuses[i].index == index) 
            {
                inuseOrder = i;
                break;
            }
        }

        if (inuseOrder != -1) 
        {
            ShopItemElement element = itemPool.inuses[inuseOrder];

            if(GameManager.instance.itemManager.money >= element.money) 
            {
                nowPickElementIndex = inuseOrder;
                GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
            }
        }
    }

    private void OnShopItemButtonUp(int index) 
    {
        ItemBaseResource pickedItem = null;
        ShopItemElement element = null;
        if (nowPickElementIndex != -1)
        {
            element = itemPool.inuses[nowPickElementIndex];
            pickedItem = element.item;
        }

        if (pickedItem != null) 
        {
            MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
            if (mainGameUI != null)
            {
                if (mainGameUI.nowEnterElementIndex >= 0)
                {
                    int addIndex = -1;
                    if (GameManager.instance.itemManager.heldItems[mainGameUI.nowEnterElementIndex] == null) 
                    {
                        addIndex = mainGameUI.nowEnterElementIndex;
                        ItemBaseResource item = GameManager.instance.itemManager.CreateItem(pickedItem.index);
                        GameManager.instance.itemManager.SetHeldItem(addIndex, item);
                    }
                    else 
                    {
                        addIndex = GameManager.instance.itemManager.AddHeldItem(pickedItem.index);
                    }

                    if (addIndex != -1)
                    {
                        //資料
                        sellItems.Remove(pickedItem);
                        GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - element.money);
                        //演出
                        /*
                        mainGameUI.elements[addIndex].isFlying = true;
                        Vector2 endPosition = mainGameUI.elements[addIndex].GlobalPosition;
                        GameManager.instance.uiManager.StartFlyItem(element.item, element.GlobalPosition, endPosition, flyTime, () =>
                        {
                            mainGameUI.elements[addIndex].isFlying = false;
                        });
                        */
                        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
                        ClearShopItemElementData(element);
                        itemPool.ReturnElement(element);
                    }
                }
            }
        }
        nowPickElementIndex = -1;
    }

    private void SetPickedItemElement(ItemBaseResource item)
    {
        if (item != null)
        {
            PickUpUI pickUpUI = GameManager.instance.uiManager.GetOpenedUI<PickUpUI>(UIIndex.PickUpUI);
            if (pickUpUI != null)
            {
                pickUpUI.SetData(item);
            }
            else
            {

                pickUpUI = GameManager.instance.uiManager.OpenUI<PickUpUI>(UIIndex.PickUpUI, new List<object>() { item });
            }
        }
        else
        {
            GameManager.instance.uiManager.CloseUI(UIIndex.PickUpUI);
        }
    }

    private void SetRefreshButton() 
    {
        if (GameManager.instance.itemManager.money >= refreshCost)
        {
            refreshButton.Disabled = false;
            refreshCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
        }
        else
        {
            refreshButton.Disabled = true;
            refreshCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
        }
        refreshCostLabel.Text = $"{refreshCost}";
    }

    private void SetRefreshHeldCost() 
    {
        if (GameManager.instance.itemManager.money >= refreshHeldCost)
        {
            refreshHeldCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
        }
        else
        {
            refreshHeldCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
        }
        refreshHeldCostLabel.Text = $"{refreshHeldCost}";
    }

    public bool RefreshHeldItem(int nowPickElementIndex) 
    {
        bool isSuccess = false;

        isSuccess = GameManager.instance.itemManager.RefreshItem(nowPickElementIndex, refreshHeldCost);
        if (isSuccess) 
        {
            refreshHeldCount++;
            SetRefreshHeldCost();
        }
        return isSuccess;
    }


    public async void OnMoveClick() 
    {
        switch (positionState) 
        {
            case PositionState.Out: 
                {
                    GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
                    positionState = PositionState.MovingIn;
                    animation.Play(clip_moveIn);
                    await ToSignal(animation, "animation_finished").ToTask();
                    positionState = PositionState.In;
                }
                break;
            case PositionState.In: 
                {
                    GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
                    positionState = PositionState.MovingOut;
                    animation.Play(clip_moveOut);
                    await ToSignal(animation, "animation_finished").ToTask();
                    positionState = PositionState.Out;
                }
                break;
        }
    }

    private void OnMoneyChange(int money) 
    {
        SetRefreshButton();
        SetRefreshHeldCost();
    }


    public void OnRefreshClick() 
    {
        RefrershItem();
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
    }

    public void OnCloseClick()
    {
        GameManager.instance.uiManager.CloseUI(this);
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_cancel4);
    }
}
