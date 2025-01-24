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
    [Export] private Control accelerationParent;
    [Export] private Label accelerationLabel;
    [Export] private Control maxSpeedParent;
    [Export] private Label maxSpeedLabel;

    private ItemBaseResource _item;
    public ItemBaseResource item 
    {
        get { return _item; }
        private set { _item = value; }
    }

    private int _index = -1;

    public int index 
    {
        get { return _index; }
        private set { _index = value; }
    }

    public event Action<int, ItemBaseResource> onDetailClick;   // <index, item>
    public event Action<int> onCloseClick;  //<index>

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public void SetIndex(int index) 
    {
        this.index = index;
    }

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
        SetUseTime();
        SetAttackPoint();
        SetAttackRange();
        SetRepairPoint();
        SetMaterial();
        SetProduct();
        SetProduceCostTime();
        SetAcceleration();
        SetMaxSpeed();
        RefreshContainerSize();
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
        for(int i = 0; i < materialPool.inuses.Count; i++) 
        {
            ItemIcon element = materialPool.inuses[i];
            element.onMainClick -= OnItemDetailClick;
        }
        materialPool.ReturnAllElement();

        if (item is IMake make)
        {
            materialParent.Visible = true;
            for(int i = 0; i < make.materials.Count; i++) 
            {
                if (GameManager.instance.itemConfig.config.TryGetValue(make.materials[i], out ItemBaseResource item)) 
                {
                    ItemIcon element = materialPool.GetElement();
                    element.onMainClick += OnItemDetailClick;
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
            productIcon.onMainClick -= OnItemDetailClick;
            productIcon.onMainClick += OnItemDetailClick;
            productIcon.SetData(productItem);
        }
        else 
        {
            productParent.Visible = false;
            productIcon.onMainClick -= OnItemDetailClick;
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

    private void SetAcceleration() 
    {
        if(item is ICore core) 
        {
            accelerationParent.Visible = true;
            accelerationLabel.Text = $"{core.acceleration}";
        }
        else 
        {
            accelerationParent.Visible = false;
        }
    }

    private void SetMaxSpeed()
    {
        if (item is ICore core)
        {
            maxSpeedParent.Visible = true;
            maxSpeedLabel.Text = $"{core.maxSpeed}";
        }
        else
        {
            maxSpeedParent.Visible = false;
        }
    }

    private void RefreshContainerSize() 
    {
        Size = CustomMinimumSize;
    }

    public void OnItemDetailClick(ItemIcon itemIcon) 
    {
        onDetailClick?.Invoke(index, itemIcon.data);
    }

    public void OnCloseClick() 
    {
        onCloseClick?.Invoke(index);
    }
}
