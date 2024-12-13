using Godot;
using System;
using System.Collections.Generic;

public partial class ItemInfoPanel : PanelContainer
{
	[Export] private ItemElement itemElement;
	[Export] private Label nameLabel;
	[Export] private Label moneyLabel;

    [Export] private Control useTimeParent;
    [Export] private Label useTimeLabel;
    [Export] private Control attackPointParent;
    [Export] private Label attackPointLabel;
    [Export] private Control attackRangeParent;
    [Export] private Label attackRangeLabel;
    [Export] private Control repairPointParent;
    [Export] private Label repairPointLabel;
    [Export] private Control materialParent;
    [Export] private ItemIconPool materialPool;
    [Export] private Control productParent;
    [Export] private ItemIcon productIcon;
    [Export] private Control produceCostTimeParent;
    [Export] private Label produceCostTimeLabel;
    [Export] private Control buttonsParent;
    [Export] private Button produceStartButton;
    [Export] private Button produceStopButton;
    [Export] private Button startUseButton;
    [Export] private Button stopUseButton;
    [Export] private Button discardButton;

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
        else if(this.item is IUseable preUseable) 
        {
            preUseable.onIsUsingChange -= OnIsUsingChange;
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
        else if (this.item is IUseable nowUseable)
        {
            nowUseable.onIsUsingChange += OnIsUsingChange;
        }

        itemElement.SetData(item);
		SetView();
    }

	private void SetView() 
	{
		SetName();
		SetMoney();
        SetUseTime();
        SetAttackPoint();
        SetAttackRange();
        SetRepairPoint();
        SetMaterial();
        SetProduct();
        SetProduceCostTime();
        SetButtons();
    }

	private void SetName() 
	{
		nameLabel.Text = $"{LanguagePrefix.itemName}{item.index}";
	}

	private void SetMoney() 
	{
		moneyLabel.Text = $"{item.money}";
    }

    private void SetUseTime() 
    {
        if (item is IUseable useable) 
        {
            useTimeParent.Visible = true;
            useTimeLabel.Text = $"{useable.needTime}";
        }
        else 
        {
            useTimeParent.Visible = false;
        }
    }

    private void SetAttackPoint()
    {
        if (item is IAttack attacker)
        {
            attackPointParent.Visible = true;
            attackPointLabel.Text = $"{attacker.attackPoint}";
        }
        else
        {
            attackPointParent.Visible = false;
        }
    }

    private void SetAttackRange() 
    {
        if (item is IAttack attacker)
        {
            attackRangeParent.Visible = true;
            attackRangeLabel.Text = $"{attacker.range}";
        }
        else
        {
            attackRangeParent.Visible = false;
        }
    }

    private void SetRepairPoint() 
    {
        if (item is IRepair repairing)
        {
            repairPointParent.Visible = true;
            repairPointLabel.Text = $"{repairing.repairPoint}";
        }
        else
        {
            repairPointParent.Visible = false;
        }
    }

    private void SetMaterial() 
    {
        materialPool.ReturnAllElement();
        if (item is IMake make)
        {
            materialParent.Visible = true;
            for(int i = 0; i < make.materials.Count; i++) 
            {
                if (GameManager.instance.itemConfig.config.TryGetValue(make.materials[i], out ItemBaseResource item)) 
                {
                    ItemIcon element = materialPool.GetElement();
                    element.SetData(item);
                }
            }
        }
        else
        {
            materialParent.Visible = false;
        }
    }

    private void SetProduct() 
    {
        if (item is IProduce produce && GameManager.instance.itemConfig.config.TryGetValue(produce.productItem, out ItemBaseResource productItem)) 
        {
            productParent.Visible = true;
            productIcon.SetData(productItem);
        }
        else 
        {
            productParent.Visible = false;
        }
    }

    private void SetProduceCostTime() 
    {
        if (item is IProduce produce)
        {
            produceCostTimeParent.Visible = true;
            produceCostTimeLabel.Text = $"{produce.needTime}";
        }
        else
        {
            produceCostTimeParent.Visible = false;
        }
    }

	private void SetButtons() 
	{
		if(item is IProduce produce) 
		{
			//buttonsParent.Visible = true;
            //discardButton.Visible = true;
            buttonsParent.Visible = false;
            discardButton.Visible = false;
            startUseButton.Visible = false;
            stopUseButton.Visible = false;

            produceStartButton.Visible = false;
            produceStopButton.Visible = false;
            /*
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
					if (make.isCostMaterial || GameManager.instance.itemManager.TryMake(make))
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
            */
        }
		else if (item is IUseable useable)
		{
            //buttonsParent.Visible = true;
            //discardButton.Visible = true;
            buttonsParent.Visible = false;
            discardButton.Visible = false;
            produceStopButton.Visible = false;
            produceStartButton.Visible = false;

            startUseButton.Visible = false;
            stopUseButton.Visible = false;
            /*
            if (useable.isUsing) 
            {
                startUseButton.Visible = false;
                stopUseButton.Visible = true;
            }
            else 
            {
                startUseButton.Visible = true;
                stopUseButton.Visible = false;
            }
            */
        }
		else 
		{
            //buttonsParent.Visible = true;
            //discardButton.Visible = true;
            buttonsParent.Visible = false;
            discardButton.Visible = false;
            startUseButton.Visible = false;
            stopUseButton.Visible = false;
            produceStopButton.Visible = false;
            produceStartButton.Visible = false;
        }
	}

	private void OnIsProducingChange(bool isProducing) 
	{
        SetButtons();
    }
    private void OnIsUsingChange(bool isUsing) 
    {
        SetButtons();
    }

	private void OnHeldItemChange(int index) 
	{
        SetButtons();
    }

    public void OnStartProduceClick() 
	{
        if (item is IProduce produce)
		{
            bool isFail = false;
            if (item is IMake make && !make.isCostMaterial)
			{
                isFail = !GameManager.instance.itemManager.Make(make);
            }

			if (!isFail)
			{
				produce.StartProduce();
				SetButtons();
			}
        }
    }

    public void OnStopProduceClick()
    {
        if (item is IProduce produce)
        {
            produce.StopProduce();
            SetButtons();
        }
    }

    public void OnStartUseClick() 
    {
        if (item is IUseable useable)
        {
            useable.StartUsing();
            SetButtons();
        }
    }

    public void OnStopUseClick()
    {
        if (item is IUseable useable)
        {
            useable.StopUsing();
            SetButtons();
        }
    }

    public void OnDiscardClick() 
    {
        if(item != null) 
        {
            for(int i = 0; i < GameManager.instance.itemManager.heldItems.Count; i++) 
            {
                if (GameManager.instance.itemManager.heldItems[i] == item) 
                {
                    GameManager.instance.itemManager.SetHeldItem(i, null);
                    break;
                }
            }
        }
    }
}
