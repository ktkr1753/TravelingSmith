using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MainGameUI : UIBase
{
    [Export] public Godot.Collections.Array<ItemElement> elements = new Godot.Collections.Array<ItemElement>();
    [Export] private AnimationPlayer animation;
    [Export] public PanelContainer itemsPanel;
    [Export] private ItemInfoPanelPool infoPanelPool;
    [Export] private ProgressBar hpProgressBar;
    [Export] private Label nowHpLabel;
    [Export] private Label maxHpLabel;
    [Export] private ProgressBar expProgressBar;
    [Export] private Control expLabelParent;
    [Export] private Label nowExpLabel;
    [Export] private Label maxExpLabel;
    [Export] private Label moneyLabel;
    [Export] private Label accelerationLabel;
    [Export] private Label speedLabel;

    //碰到的物件ID
    private int _nowEnterElementIndex = -1;
    public int nowEnterElementIndex 
    {
        get { return _nowEnterElementIndex; }
        set 
        {
            if (_nowEnterElementIndex != value) 
            {
                _nowEnterElementIndex = value;
                SetItemSelectedState(nowEnterElementIndex, nowSelectedElementIndex);
            }
        }
    }

    //點選的物件ID
    private int _nowSelectedElementIndex = -1;

    public int nowSelectedElementIndex 
    {
        get { return _nowSelectedElementIndex; }
        set 
        {
            if (_nowSelectedElementIndex != value) 
            {
                //Debug.Print($"nowSelectedElementIndex change:{value}");

                int preSelectedElementIndex = _nowSelectedElementIndex;
                if (preSelectedElementIndex >= 0 && preSelectedElementIndex < elements.Count) 
                {
                    elements[preSelectedElementIndex].SetDropButton(false);
                }
                _nowSelectedElementIndex = value;
                SetItemSelectedState(nowEnterElementIndex, nowSelectedElementIndex);
            }
            if(_nowSelectedElementIndex >= 0 && _nowSelectedElementIndex < GameManager.instance.itemManager.heldItems.Count) 
            {
                Vector2 globalPos = new Vector2();
                globalPos = GetInfoPanelPos(0);
                SetInfoPanel(0, GameManager.instance.itemManager.heldItems[nowEnterElementIndex], globalPos);
            }
        }
    }

    private Vector2? recordClickPos = null;

    //拉動的物件ID
    private int _nowPickElementIndex = -1;
    public int nowPickElementIndex 
    {
        get { return _nowPickElementIndex; }
        set 
        {
            if(_nowPickElementIndex != value) 
            {
                int preState = _nowPickElementIndex;
                _nowPickElementIndex = value;

                if (preState != -1) 
                {
                    elements[preState].isPicking = false;
                }

                if(_nowPickElementIndex != -1) 
                {
                    SetPickedItemElement(GameManager.instance.itemManager.heldItems[_nowPickElementIndex]);
                    elements[_nowPickElementIndex].isPicking = true;
                    GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_1);
                }
                else 
                {
                    SetPickedItemElement(null);
                }
            }
        }
    }

    public const float moveThreshold = 5;
    public const double flyTime = 0.2;
    public readonly Vector2 infoPanelFix = new Vector2(12, 12);

    public override void Init()
    {
        base.Init();
        GameManager.instance.itemManager.onMoneyChange += OnMoneyChange;
        GameManager.instance.itemManager.onMoveAccelerationChange += OnAccelerationChange;
        GameManager.instance.itemManager.onMoveSpeedChange += OnSpeedChange;
        GameManager.instance.itemManager.onHeldItemChange += OnHeldItemChange;
        GameManager.instance.itemManager.onItemEffectsChange += OnItemEffectChange;
        GameManager.instance.itemManager.onUseMaterial += OnUseMaterials;
        GameManager.instance.itemManager.onCreateProduct += OnCreateProduct;
        GameManager.instance.battleManager.onHPChange += OnHpChange;
        GameManager.instance.battleManager.onExpChange += OnExpChange;
        GameManager.instance.battleManager.onLevelChange += OnLevelChange;
        InitItemElements();
        SetView();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        GameManager.instance.itemManager.onMoneyChange -= OnMoneyChange;
        GameManager.instance.itemManager.onMoveAccelerationChange -= OnAccelerationChange;
        GameManager.instance.itemManager.onMoveSpeedChange -= OnSpeedChange;
        GameManager.instance.itemManager.onHeldItemChange -= OnHeldItemChange;
        GameManager.instance.itemManager.onItemEffectsChange -= OnItemEffectChange;
        GameManager.instance.itemManager.onUseMaterial -= OnUseMaterials;
        GameManager.instance.itemManager.onCreateProduct -= OnCreateProduct;
        GameManager.instance.battleManager.onHPChange -= OnHpChange;
        GameManager.instance.battleManager.onExpChange -= OnExpChange;
        GameManager.instance.battleManager.onLevelChange -= OnLevelChange;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        CheckIsPickItem();
        CheckItemsWork();
    }

    
    private void SetView()
    {
        SetHp();
        SetExp();
        SetMoney();
    }

    private void CheckIsPickItem() 
    {
        if(recordClickPos != null) 
        {
            Vector2 nowPos = GetViewport().GetMousePosition();
            if(recordClickPos.GetValueOrDefault(Vector2.Zero).DistanceTo(nowPos) >= moveThreshold && nowSelectedElementIndex != -1) 
            {
                if(GameManager.instance.itemManager.heldItems[nowSelectedElementIndex] != null) 
                {
                    nowPickElementIndex = nowSelectedElementIndex;
                    ReturnInfoPanelElement(0);
                }
                else 
                {
                    recordClickPos = null;
                }
            }
        }
        else 
        {
            nowPickElementIndex = -1;
        }
    }

    private void CheckItemsWork() 
    {
        for(int i = 0; i < GameManager.instance.itemManager.heldItems.Count && i < GameManager.instance.itemManager.areas.Count; i++) 
        {
            bool isProduce = false;
            bool isUsing = false;
            bool isCore = false;
            bool isFire = false;

            ItemBaseResource item = GameManager.instance.itemManager.heldItems[i];
            if (GameManager.instance.itemManager.IsAreaEffect(GameManager.instance.itemManager.areas, i, AreaIndex.Produce))
            {
                isProduce = true;
            }
            if (GameManager.instance.itemManager.IsAreaEffect(GameManager.instance.itemManager.areas, i, AreaIndex.Useable))
            {
                isUsing = true;
            }
            if (GameManager.instance.itemManager.IsAreaEffect(GameManager.instance.itemManager.areas, i, AreaIndex.Core))
            {
                isCore = true;
            }
            if(GameManager.instance.itemManager.IsAreaEffect(GameManager.instance.itemManager.areas, i, AreaIndex.Fire)) 
            {
                isFire = true;
            }

            if (item is IProduce produce)
            {
                switch (produce.type) 
                {
                    case ProduceType.Produce:
                        {
                            if (isProduce && !produce.isProducing) 
                            {
                                if (item is IMake make) 
                                {
                                    bool isFail = false;
                                    if (!make.isCostMaterial)
                                    {
                                        isFail = !GameManager.instance.itemManager.Make(make, elements);
                                    }
                                    if (!isFail)
                                    {
                                        produce.StartProduce();
                                    }
                                }
                                else 
                                {
                                    produce.StartProduce();
                                }
                            }
                            else if (!isProduce && produce.isProducing)
                            {
                                produce.StopProduce();
                            }
                        }
                        break;
                    case ProduceType.Fire: 
                        {
                            if (isFire && !produce.isProducing) 
                            {
                                if (item is IMake make)
                                {
                                    bool isFail = false;
                                    if (!make.isCostMaterial)
                                    {
                                        isFail = !GameManager.instance.itemManager.Make(make, elements);
                                    }
                                    if (!isFail)
                                    {
                                        produce.StartProduce();
                                    }
                                }
                                else 
                                {
                                    produce.StartProduce();
                                }
                            }
                            else if (!isFire && produce.isProducing)
                            {
                                produce.StopProduce();
                            }
                        }
                        break;
                }
            }
            else if (item is IUseable useable) 
            {
                if (isUsing && !useable.isUsing)
                {
                    useable.StartUsing();
                }
                else if(!isUsing && useable.isUsing)
                {
                    useable.StopUsing();
                }
            }
            else if(item is ICore core) 
            {
                if (isCore && !core.isUsing) 
                {
                    core.StartCore();
                }
                else if (!isCore && core.isUsing) 
                {
                    core.StopCore();
                }
            }
        }
    }


    private void InitItemElements() 
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].SetData(i, GameManager.instance.itemManager.heldItems[i]);
            elements[i].SetArea(GameManager.instance.itemManager.areas[i]);
            elements[i].onMouseEnter += OnElementMouseEnter;
            elements[i].onMouseExit += OnElementMouseExit;
            elements[i].onMainButtonDown += OnElementButtonDown;
            elements[i].onMainButtonUp += OnElementButtonUp;
            elements[i].onMainRightPressed += OnElementButtonRightPressed;
            elements[i].onDropPressed += OnElementDropButtonPressed;
        }
    }

    public void RefreshItemElement(int index, ItemBaseResource item)
    {
		elements[index].SetData(index, item);
    }

    private void RefreshItemEffect(int index, AreaIndex area) 
    {
        elements[index].SetArea(area);
    }

    private void SetHp() 
    {
        hpProgressBar.MaxValue = GameManager.instance.battleManager.maxHP;
        hpProgressBar.Value = GameManager.instance.battleManager.nowHP;
        maxHpLabel.Text = $"{GameManager.instance.battleManager.maxHP}";
        nowHpLabel.Text = $"{GameManager.instance.battleManager.nowHP}";
    }

    private void SetExp() 
    {
        int nowLevel = GameManager.instance.battleManager.nowLevel;
        int maxLevel = GameManager.instance.battleManager.maxLevel;
        if (nowLevel == maxLevel) 
        {
            expProgressBar.MaxValue = 100;
            expProgressBar.Value = 100;
            expLabelParent.Visible = false;
        }
        else 
        {
            int expInterval = GameManager.instance.expConfig.expIntervals[nowLevel];
            int preNeedExp = 0;
            if (nowLevel > 0) 
            {
                preNeedExp = GameManager.instance.expConfig.expAllIntervals[nowLevel - 1];
            }

            expProgressBar.MaxValue = expInterval;
            expProgressBar.Value = GameManager.instance.battleManager.nowExp - preNeedExp;
            expLabelParent.Visible = true;
            maxExpLabel.Text = $"{expProgressBar.MaxValue}";
            nowExpLabel.Text = $"{expProgressBar.Value}";
        }
    }

    private void SetMoney() 
    {
        moneyLabel.Text = $"{GameManager.instance.itemManager.money}";
    }

    private void SetAcceleration() 
    {
        accelerationLabel.Text = $"{Math.Max(0, GameManager.instance.itemManager.moveAcceleration)}";
    }

    private void SetSpeed() 
    {
        speedLabel.Text = $"{ Math.Floor(GameManager.instance.itemManager.moveSpeed)}";
    }

    private void SetItemSelectedState(int nowEnterElementIndex, int nowSelectedElementIndex)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (i == nowSelectedElementIndex)
            {
                elements[i].nowSelectedState = ItemElement.SelectedState.Selected;
            }
            else if (i == nowEnterElementIndex)
            {
                elements[i].nowSelectedState = ItemElement.SelectedState.Hover;
            }
            else
            {
                elements[i].nowSelectedState = ItemElement.SelectedState.None;
            }
        }
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


    private void SetInfoPanel(int index, ItemBaseResource newItem, Vector2? globalPos = null) 
    {
        if(infoPanelPool.inuses.Count > index)
        {
            if(newItem != null) 
            {
                infoPanelPool.inuses[index].SetData(newItem);
                if(globalPos != null)
                {
                    infoPanelPool.inuses[index].GlobalPosition = globalPos.GetValueOrDefault();
                }
                ReturnInfoPanelElement(index + 1);
            }
            else 
            {
                ReturnInfoPanelElement(index);
            }
        }
        else if(globalPos != null)
        {
            if (newItem != null)
            {
                int newIndex = infoPanelPool.inuses.Count;
                ItemInfoPanel infoPanel = infoPanelPool.GetElement();
                infoPanel.onDetailClick += OnInfoDetailClick;
                infoPanel.onCloseClick += OnInfoCloseClick;
                infoPanel.SetData(newItem);
                infoPanel.SetIndex(newIndex);
                if (globalPos != null)
                {
                    infoPanelPool.inuses[index].GlobalPosition = globalPos.GetValueOrDefault();
                }
            }
            else
            {
                ReturnInfoPanelElement(index);
            }
        }
        else 
        {
            ReturnInfoPanelElement(index);
        }
    }
    private void ReturnInfoPanelElement(int index) 
    {
        for(int i = infoPanelPool.inuses.Count - 1; i >= index; i--) 
        {
            infoPanelPool.inuses[i].onDetailClick -= OnInfoDetailClick;
            infoPanelPool.inuses[i].onCloseClick -= OnInfoCloseClick;
            infoPanelPool.ReturnElement(infoPanelPool.inuses[i]);
        }
    }


    public Vector2 GetInfoPanelPos(int index) 
    {
        Vector2 result = new Vector2(30 + (nowSelectedElementIndex % ItemManager.itemColumnNum) * 28 + index * 25, 170 + (nowSelectedElementIndex / ItemManager.itemColumnNum) * 28  + index * 25);
        return result;
    }

    private void OnElementMouseEnter(int index) 
    {
        if(index != nowEnterElementIndex) 
        {
            nowEnterElementIndex = index;
            //Debug.Print($"OnElementMouseEnter index:{nowEnterElementIndex}");

            if (nowPickElementIndex != -1)
            {
                Godot.Collections.Array<AreaIndex> tempAreas = GameManager.instance.itemManager.TempAreaChange(GameManager.instance.itemManager.heldItems[nowPickElementIndex], _nowEnterElementIndex, nowPickElementIndex);
                for(int i = 0; i < tempAreas.Count; i++) 
                {
                    RefreshItemEffect(i, tempAreas[i]);
                }
            }
            else 
            {
                ShopUI shopUI = GameManager.instance.uiManager.GetOpenedUI<ShopUI>(UIIndex.ShopUI);
                if (shopUI != null && shopUI.nowPickElementIndex != -1)
                {
                    Godot.Collections.Array<AreaIndex> tempAreas = GameManager.instance.itemManager.TempAreaChange(shopUI.sellItems[shopUI.nowPickElementIndex], _nowEnterElementIndex);
                    for (int i = 0; i < tempAreas.Count; i++)
                    {
                        RefreshItemEffect(i, tempAreas[i]);
                    }
                }
            }
        }
    }

    private void OnElementMouseExit(int index) 
    {
        if(index == nowEnterElementIndex) 
        {
            nowEnterElementIndex = -1;
            //Debug.Print($"OnElementMouseExit index:{nowEnterElementIndex}");
            for (int i = 0; i < GameManager.instance.itemManager.areas.Count; i++)
            {
                RefreshItemEffect(i, GameManager.instance.itemManager.areas[i]);
            }
        }
    }

    private void OnElementButtonDown(int index) 
    {
        nowSelectedElementIndex = index;

        recordClickPos = GetViewport().GetMousePosition();

        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
        //GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_1);
    }

    private void OnElementButtonUp(int index) 
    {
        ItemBaseResource pickedItem = null;
        ItemBaseResource putItem = null;
        if (nowPickElementIndex != -1) 
        {
            pickedItem = GameManager.instance.itemManager.heldItems[nowPickElementIndex];
        }

        if (pickedItem != null)
        {
            for (int i = 0; i < GameManager.instance.itemManager.areas.Count; i++)
            {
                RefreshItemEffect(i, GameManager.instance.itemManager.areas[i]);
            }
           
            if (nowEnterElementIndex != -1 && nowEnterElementIndex != nowPickElementIndex
                && GameManager.instance.itemManager.IsCanPut(nowEnterElementIndex, pickedItem)) 
            {
                putItem = GameManager.instance.itemManager.heldItems[nowEnterElementIndex];
                GameManager.instance.itemManager.SetHeldItem(nowPickElementIndex, putItem);
                GameManager.instance.itemManager.SetHeldItem(nowEnterElementIndex, pickedItem);

                //飛行演出
                Vector2 point1 = elements[nowPickElementIndex].GlobalPosition;
                Vector2 point2 = elements[nowEnterElementIndex].GlobalPosition;
                //被替換的飛行
                elements[nowPickElementIndex].isFlying = true;
                int tempNowPickElementIndex = nowPickElementIndex;
                GameManager.instance.uiManager.StartFlyItem(putItem, point2, point1, flyTime, () =>
                {
                    elements[tempNowPickElementIndex].isFlying = false;
                });
                GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_2);
            }
            else
            {
                ShopUI shopUI = GameManager.instance.uiManager.GetOpenedUI<ShopUI>(UIIndex.ShopUI);
                if(shopUI != null) 
                {
                    Vector2 mousePos = GetViewport().GetMousePosition();
                    if (shopUI.IsInSellRect(mousePos)) 
                    {
                        bool isSuccess = GameManager.instance.itemManager.SellItem(nowPickElementIndex);
                        //Debug.Print($"Sell Shop, isSuccess:{isSuccess}");
                        if (isSuccess) 
                        {
                            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_get_money);
                        }
                    }
                    else if (shopUI.IsInRefreshRect(mousePos)) 
                    {
                        bool isSuccess = shopUI.RefreshHeldItem(nowPickElementIndex);
                        if (isSuccess)
                        {
                            //飛行演出
                            putItem = GameManager.instance.itemManager.heldItems[nowPickElementIndex];
                            Vector2 point1 = mousePos;
                            Vector2 point2 = elements[nowPickElementIndex].GlobalPosition;
                            //被替換的飛行
                            elements[nowPickElementIndex].isFlying = true;
                            int tempNowPickElementIndex = nowPickElementIndex;
                            GameManager.instance.uiManager.StartFlyItem(putItem, point1, point2, flyTime, () =>
                            {
                                elements[tempNowPickElementIndex].isFlying = false;
                            });
                            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_get_money);
                        }
                    }
                }
            }
        }

        recordClickPos = null;

        if(nowEnterElementIndex != -1)
        {
            nowSelectedElementIndex = nowEnterElementIndex;
        }
        //Debug.Print($"OnElementButtonUp index:{nowSelectedElementIndex}");
    }

    private void OnElementButtonRightPressed(int index) 
    {
        nowSelectedElementIndex = index;

        if (index >= 0 && index < GameManager.instance.itemManager.heldItems.Count 
            && GameManager.instance.itemManager.heldItems[index] != null) 
        {
            elements[index].SetDropButton(true);
        }
    }

    private void OnElementDropButtonPressed(int index) 
    {
        if (index >= 0 && index < GameManager.instance.itemManager.heldItems.Count
            && GameManager.instance.itemManager.heldItems[index] != null)
        {
            if(GameManager.instance.itemManager.waitAddItem != null) 
            {
                ItemBaseResource item = GameManager.instance.itemManager.CreateItem(GameManager.instance.itemManager.waitAddItem.index);
                GameManager.instance.itemManager.SetHeldItem(index, item);
                GameManager.instance.itemManager.waitAddItem = null;

                UIBase dropUI = GameManager.instance.uiManager.GetOpenedUI(UIIndex.DropItemUI);
                if (dropUI != null) 
                {
                    GameManager.instance.uiManager.CloseUI(dropUI);

                    GameManager.instance.uiManager.OpenUI(UIIndex.ShopUI);
                }

                /*
                UIBase pickUI = GameManager.instance.uiManager.GetOpenedUI(UIIndex.PickUI);
                if (pickUI != null)
                {
                    GameManager.instance.uiManager.CloseUI(pickUI);
                }
                */
            }
            else 
            {
                GameManager.instance.itemManager.RemoveItem(index);
            }
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button19);
            elements[index].SetDropButton(false);
        }
    }

    private void OnHeldItemChange(int index) 
    {
        RefreshItemElement(index, GameManager.instance.itemManager.heldItems[index]);

        if (index == nowPickElementIndex) 
        {
            ItemBaseResource item = GameManager.instance.itemManager.heldItems[nowPickElementIndex];
            SetPickedItemElement(item);
            SetInfoPanel(0, item);
        }
        else if(index == nowSelectedElementIndex) 
        {
            ItemBaseResource item = GameManager.instance.itemManager.heldItems[nowSelectedElementIndex];
            SetInfoPanel(0, item);
        }
    }

    private void OnItemEffectChange(int index) 
    {
        RefreshItemEffect(index, GameManager.instance.itemManager.areas[index]);
    }

    private void OnHpChange(int preHP,int nowHP, HPChangeType type) 
    {
        SetHp();
    }
    private void OnExpChange(int preExp, int nowExp)
    {
        SetExp();
    }

    private void OnUseMaterials(IMake make, HashSet<KeyValuePair<int, ItemBaseResource>> usedMaterials) 
    {
        int endIndex = -1;
        for (int i = 0; i < GameManager.instance.itemManager.heldItems.Count; i++) 
        {
            if (GameManager.instance.itemManager.heldItems[i] == make) 
            {
                endIndex = i;            
            }
        }

        if(endIndex != -1) 
        {
            foreach(KeyValuePair<int, ItemBaseResource> KV in usedMaterials) 
            {
                Vector2 startPoint = elements[KV.Key].GlobalPosition;
                Vector2 endPoint = elements[endIndex].GlobalPosition;
                ItemBaseResource item = KV.Value;
                GameManager.instance.uiManager.StartFlyItem(item, startPoint, endPoint, flyTime);
            }
        }
    }

    private void OnCreateProduct(IProduce produce, int endIndex, ItemBaseResource item) 
    {
        for(int i = 0; i < GameManager.instance.itemManager.heldItems.Count; i++) 
        {
            if (GameManager.instance.itemManager.heldItems[i] == produce) 
            {
                int startIndex = i;
                Vector2 startPoint = elements[startIndex].GlobalPosition;
                Vector2 endPoint = elements[endIndex].GlobalPosition;
                elements[endIndex].isFlying = true;
                GameManager.instance.uiManager.StartFlyItem(item, startPoint, endPoint, flyTime, () =>
                {
                    elements[endIndex].isFlying = false;
                });
            }
        }
    }


    private void OnMoneyChange(int money) 
    {
        SetMoney();
    }
    private void OnAccelerationChange(double acceleration) 
    {
        SetAcceleration();
    }

    private void OnSpeedChange(double speed) 
    {
        SetSpeed();
    }

    private void OnLevelChange(int preLevel, int nextLevel) 
    {
        if(nextLevel >= preLevel) 
        {
            for(int i = preLevel; i < nextLevel; i++) 
            {
                List<ItemBaseResource> canPickItems = GameManager.instance.itemManager.GetPickItems(3);
                GameManager.instance.uiManager.OpenUI(UIIndex.PickUI, new List<object>() { canPickItems });
            }
        }
    }

    public void OnInfoDetailClick(int index, ItemBaseResource item) 
    {
        int newIndex = index + 1;
        Vector2 globalPos = GetInfoPanelPos(newIndex);
        SetInfoPanel(newIndex, item, globalPos);
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
    }

    public void OnInfoCloseClick(int index) 
    {
        ReturnInfoPanelElement(index);
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_cancel4);
    }
}
