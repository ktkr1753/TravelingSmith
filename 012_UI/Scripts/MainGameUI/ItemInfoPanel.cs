using Godot;
using System;

public partial class ItemInfoPanel : PanelContainer
{
	[Export] private MainGameItemElement itemElement;
	[Export] private Label nameLabel;
	[Export] private Label moneyLabel;

	private ItemBaseResource item;

	public void SetData(ItemBaseResource item) 
	{
		this.item = item;
		if(item == null) 
		{
			return;
		}

		itemElement.SetData(item);
		SetView();
    }

	private void SetView() 
	{
		SetName();
		SetMoney();
    }

	private void SetName() 
	{
		nameLabel.Text = $"{LanguagePrefix.itemName}{item.index}";
	}

	private void SetMoney() 
	{
		moneyLabel.Text = $"{item.money}";

    }
}
