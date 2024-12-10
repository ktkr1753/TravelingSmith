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

    private MainGameItemElement _itemElement;
    [Export] public MainGameItemElement itemElement 
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
    }

    private void SetMoney() 
    {
        if(item != null) 
        {
            moneyLabel.Text = $"{money}";
        }
    }
}
