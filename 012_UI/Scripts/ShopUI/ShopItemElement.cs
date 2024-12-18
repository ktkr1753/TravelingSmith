using Godot;
using System;

public partial class ShopItemElement : Control
{
    private int _index = -1;
    [Export] public int index 
    {
        get { return _index; }
        private set { _index = value; }
    }

    private ItemElement _itemElement;
    [Export] public ItemElement itemElement 
    {
        get { return _itemElement; }
        private set { _itemElement = value; }
    }

    [Export] private Label moneyLabel;


    private ItemBaseResource _item;
    public ItemBaseResource item
    {
        get { return _item; }
        private set { _item = value; }
    }

    private int _money = 0;
    public int money 
    {
        get { return _money; }
        private set { _money = value; }
    }

    public override void _Ready()
    {
        base._Ready();

        GameManager.instance.itemManager.onMoneyChange += OnMoneyChange;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.itemManager.onMoneyChange -= OnMoneyChange;
    }

    public void SetData(int index, ItemBaseResource item) 
    {
        this.index = index;
        this.item = item;
        itemElement.SetData(index, item);
        InitMoney();
        SetView();
    }



    private void InitMoney() 
    {
        if(item != null) 
        {
            money = item.money * 5;
        }
    }



    private void SetView() 
    {
        SetMoney();
        SetMoneyColor();
    }

    private void SetMoney() 
    {
        if(item != null) 
        {
            moneyLabel.Text = $"{money}";
        }
    }

    private void SetMoneyColor() 
    {
        if (item != null)
        {
            if(GameManager.instance.itemManager.money >= money) 
            {
                moneyLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
            }
            else 
            {
                moneyLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
            }
        }
    }

    private void OnMoneyChange(int money) 
    {
        SetMoneyColor();
    }
}
