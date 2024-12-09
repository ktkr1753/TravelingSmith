using Godot;
using System;

public partial class ShopItemElement : Control
{
    [Export] private int index = -1;

    [Export] private MainGameItemElement itemElement;

    [Export] private Label moneyLabel;


    private ItemBaseResource _item;
    public ItemBaseResource item
    {
        get { return _item; }
        private set { _item = value; }
    }


    public void SetData(int index, ItemBaseResource item) 
    {
        this.index = index;
        this.item = item;
        itemElement.SetData(item);

        SetView();
    }

    private void SetView() 
    {
        SetMoney();
    }

    private void SetMoney() 
    {
        moneyLabel.Text = $"{item.money}";
    }
}
