using Godot;
using System;
using System.Collections.Generic;

public partial class MainGameUI : UIBase
{
	[Export] public Godot.Collections.Array<ItemElement> elements = new Godot.Collections.Array<ItemElement>();
    [Export] private ItemElement pickItemElement;
    [Export] private ItemInfoPanel infoPanel;
    [Export] private ProgressBar hpProgressBar;
    [Export] private Label nowHpLabel;
    [Export] private Label maxHpLabel;
    [Export] private ProgressBar expProgressBar;
    [Export] private Control expLabelParent;
    [Export] private Label nowExpLabel;
    [Export] private Label maxExpLabel;
    [Export] private Label moneyLabel;

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
                _nowSelectedElementIndex = value;
                SetItemSelectedState(nowEnterElementIndex, nowSelectedElementIndex);
                if(_nowSelectedElementIndex >= 0 && _nowSelectedElementIndex < GameManager.instance.itemManager.heldItems.Count) 
                {
                    SetInfoPanel(GameManager.instance.itemManager.heldItems[nowEnterElementIndex]);
                }
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
                }
                else 
                {
                    SetPickedItemElement(null);
                }
            }

            _nowPickElementIndex = value; 
        }
    }

    public const float moveThreshold = 5;

    public const double flyTime = 0.2;

    public override void Init()
    {
        base.Init();
        GameManager.instance.itemManager.onMoneyChange += OnMoneyChange;
        GameManager.instance.itemManager.onHeldItemChange += OnHeldItemChange;
        GameManager.instance.itemManager.onUseMaterial += OnUseMaterials;
        GameManager.instance.itemManager.onCreateProduct += OnCreateProduct;
        GameManager.instance.battleManager.onHPChange += OnHpChange;
        GameManager.instance.battleManager.onExpChange += OnExpChange;
        GameManager.instance.battleManager.onLevelChange += OnLevelChange;
        InitItems();
        InitElements();
        RefreshItemElements();
        SetHp();
        SetExp();
        SetMoney();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        GameManager.instance.itemManager.onMoneyChange -= OnMoneyChange;
        GameManager.instance.itemManager.onHeldItemChange -= OnHeldItemChange;
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
        UpdateSelectedItemImagePos();
    }

    private void CheckIsPickItem() 
    {
        if(recordClickPos != null) 
        {
            Vector2 nowPos = GetViewport().GetMousePosition();
            if(recordClickPos.GetValueOrDefault(Vector2.Zero).DistanceTo(nowPos) >= moveThreshold && nowSelectedElementIndex != -1) 
            {
                nowPickElementIndex = nowSelectedElementIndex;
            }
            else 
            {
                nowPickElementIndex = -1;
            }
        }
        else 
        {
            nowPickElementIndex = -1;
        }
    }

    private void InitItems() 
    {
        for (int i = 0; i < GameManager.instance.itemManager.heldItems.Count; i++)
        {
            ItemBaseResource item = GameManager.instance.itemManager.heldItems[i];
        }
    }

    private void InitElements() 
    {
        for(int i = 0; i < elements.Count; i++) 
        {
            elements[i].onMouseEnter += OnElementMouseEnter;
            elements[i].onMouseExit += OnElementMouseExit;
            elements[i].onMainButtonDown += OnElementButtonDown;
            elements[i].onMainButtonUp += OnElementButtonUp;
        }
    }

    public void RefreshItemElements() 
	{
		for(int i = 0; i < elements.Count; i++) 
		{
            if (nowSelectedElementIndex == i) 
            {
                RefreshItemElement(i, null);
            }
            else 
            {
    			RefreshItemElement(i, GameManager.instance.itemManager.heldItems[i]);
            }
        }
	}

    public void RefreshItemElement(int index, ItemBaseResource item)
    {
		elements[index].SetData(item);
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
            pickItemElement.Visible = true;
        }
        else 
        {
            pickItemElement.Visible = false;
        }
        pickItemElement.SetData(item);
    }

    private void SetInfoPanel(ItemBaseResource item) 
    {
        if(item != null) 
        {
            infoPanel.Visible = true;
            infoPanel.SetData(item);
        }
        else 
        {
            infoPanel.Visible = false;
        }
    }

    private void UpdateSelectedItemImagePos() 
    {
        if (pickItemElement.Visible) 
        {
            pickItemElement.GlobalPosition = GetViewport().GetMousePosition() - new Vector2(pickItemElement.Size.X / 2, pickItemElement.Size.Y / 2);
        }
    }

    private void OnElementMouseEnter(int index) 
    {
        if(index != nowEnterElementIndex) 
        {
            nowEnterElementIndex = index;
            //Debug.Print($"OnElementMouseEnter index:{nowEnterElementIndex}");
        }
    }

    private void OnElementMouseExit(int index) 
    {
        if(index == nowEnterElementIndex) 
        {
            nowEnterElementIndex = -1;
            //Debug.Print($"OnElementMouseExit index:{nowEnterElementIndex}");
        }
    }

    private void OnElementButtonDown(int index) 
    {
        nowSelectedElementIndex = index;

        recordClickPos = GetViewport().GetMousePosition();
    }

    private void OnElementButtonUp(int index) 
    {
        ItemBaseResource pickedItem = null;
        ItemBaseResource putItem = null;
        if (nowPickElementIndex != -1) 
        {
            pickedItem = GameManager.instance.itemManager.heldItems[nowPickElementIndex];
        }

        if (nowEnterElementIndex != -1 && pickedItem != null) 
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
        }
        else 
        {

        }

        recordClickPos = null;

        if(nowEnterElementIndex != -1)
        {
            nowSelectedElementIndex = nowEnterElementIndex;
        }
        //Debug.Print($"OnElementButtonUp index:{nowSelectedElementIndex}");
    }

    private void OnHeldItemChange(int index) 
    {
        RefreshItemElement(index, GameManager.instance.itemManager.heldItems[index]);
        
        if(index == nowPickElementIndex) 
        {
            ItemBaseResource item = GameManager.instance.itemManager.heldItems[nowPickElementIndex];
            SetPickedItemElement(item);
            SetInfoPanel(item);
        }
        else if(index == nowSelectedElementIndex) 
        {
            ItemBaseResource item = GameManager.instance.itemManager.heldItems[nowSelectedElementIndex];
            SetInfoPanel(item);
        }
    }

    private void OnHpChange(int nowHP) 
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
}
