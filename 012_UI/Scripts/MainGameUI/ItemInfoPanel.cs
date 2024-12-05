using Godot;
using System;
using System.Collections.Generic;

public partial class ItemInfoPanel : PanelContainer
{
	[Export] private MainGameItemElement itemElement;
	[Export] private Label nameLabel;
	[Export] private Label moneyLabel;
	[Export] private Control produceButtonsParent;
    [Export] private Button produceStartButton;
    [Export] private Button produceStopButton;

    private ItemBaseResource item;

    public override void _Ready()
    {
        base._Ready();

		GameManager.instance.itemManager.onHeldItemChange += OnHeldItemChange;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.itemManager.onHeldItemChange -= OnHeldItemChange;
    }

    public void SetData(ItemBaseResource item) 
	{
		if(this.item is IProduce preProduce) 
		{
            preProduce.onIsProducingChange -= OnIsProducingChange;
        }

		this.item = item;
		if(item == null) 
		{
			return;
		}

        if (item is IProduce nowProduce)
        {
            nowProduce.onIsProducingChange += OnIsProducingChange;
        }
        itemElement.SetData(item);
		SetView();
    }

	private void SetView() 
	{
		SetName();
		SetMoney();
		SetProduceButtons();
    }

	private void SetName() 
	{
		nameLabel.Text = $"{LanguagePrefix.itemName}{item.index}";
	}

	private void SetMoney() 
	{
		moneyLabel.Text = $"{item.money}";
    }

	private void SetProduceButtons() 
	{
		if(item is IProduce produce) 
		{
			produceButtonsParent.Visible = true;
			if (produce.isProducing) 
			{
				produceStopButton.Visible = true;
                produceStartButton.Visible = false;
            }
			else 
			{
                produceStartButton.Visible = true;
                produceStopButton.Visible = false;

				if(produce is IMake make) 
				{
					if (GameManager.instance.itemManager.TryMake(make))
					{
						produceStartButton.Disabled = false;
                    }
					else 
					{
                        produceStartButton.Disabled = true;
                    }
				}
				else 
				{
                    produceStartButton.Disabled = false;
                }
            }
        }
		else 
		{
            produceButtonsParent.Visible = false;
        }
	}

	private void OnIsProducingChange(bool isProducing) 
	{
        SetProduceButtons();
    }

	private void OnHeldItemChange(int index) 
	{
        SetProduceButtons();
    }


    public void OnStartProduceClick() 
	{
        if (item is IProduce produce)
		{
            bool isFail = false;
            if (item is IMake make && !make.isProducing)
			{
				bool isSuccess = GameManager.instance.itemManager.TryMake(make, out HashSet<int> usedItemsIndex);
				isFail = !isSuccess;

                if (!isFail) 
				{
					foreach(int index in usedItemsIndex) 
					{
						GameManager.instance.itemManager.SetHeldItem(index, null);
                    }
				}
			}

			if (!isFail)
			{
				produce.StartProduce();
				SetProduceButtons();
			}
			Debug.Print("OnStartProduceClick");
        }
    }

    public void OnStopProduceClick()
    {
        if (item is IProduce produce)
        {
            produce.StopProduce();
            SetProduceButtons();
            Debug.Print("OnStopProduceClick");
        }
    }
}
