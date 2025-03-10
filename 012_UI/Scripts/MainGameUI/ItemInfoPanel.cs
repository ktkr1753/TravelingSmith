using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class ItemInfoPanel : PanelContainer
{
    [Export] private AnimationPlayer anim;
	[Export] private ItemElement itemElement;
	[Export] private Label nameLabel;
    [Export] private Control moneyNode;
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
    [Export] private TextureButton productLeftButton;
    [Export] private TextureButton productRightButton;
    [Export] private Control produceCostTimeParent;
    [Export] private Label produceCostTimeLabel;
    [Export] private Control accelerationParent;
    [Export] private Label accelerationLabel;
    [Export] private Control maxSpeedParent;
    [Export] private Label maxSpeedLabel;
    [Export] private Control needAreaParent;
    [Export] private TextureRect needAreaImage;
    [Export] private Control makeAreaParent;
    [Export] private TextureRect makeAreaImage;
    [Export] private Control keepProduceParent;
    [Export] private Button keepProduceButton;


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


    private bool _interactable = true;
    public bool interactable 
    {
        get { return _interactable; }
        private set { _interactable = value; }
    }

    public event Action<int, ItemBaseResource> onDetailClick;   // <index, item>
    public event Action<int> onCloseClick;  //<index>

    public const string clip_fadeIn = "fadeIn";
    public const string clip_fadeOut = "fadeOut";

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

    public void SetData(ItemBaseResource item, bool interactable = true) 
	{
        if(this.item != null) 
        {
            UnregisterEvent(this.item);
        }
		this.item = item;
        this.interactable = interactable;
        if (item == null) 
		{
			return;
		}
        RegisterEvent(this.item);
        itemElement.SetData(item);
		SetView();
    }
    private void RegisterEvent(ItemBaseResource item) 
    {
        if (item is IAttack attacker)
        {
            attacker.onDurabilityChange += OnDurabilityChange;
        }
        if (item is ProduceResource produce) 
        {
            produce.onParameterIndexChange += OnParameterIndexChange;
        }
    }

    private void UnregisterEvent(ItemBaseResource item) 
    {
        if (item is IAttack attacker)
        {
            attacker.onDurabilityChange -= OnDurabilityChange;
        }
        if (item is ProduceResource produce)
        {
            produce.onParameterIndexChange -= OnParameterIndexChange;
        }
    }

    public async Task PlayFadeIn() 
    {
        anim.Stop();
        anim.Play(clip_fadeIn);
        await ToSignal(anim, "animation_finished").ToTask();
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
        SetNeedArea();
        SetMakeArea();
        SetKeepProduce();
        //RefreshContainerSize();
        //ResetContainerPos();
    }

	private void SetName() 
	{
		nameLabel.Text = $"{LanguagePrefix.itemName}{item.index}";
	}

	private void SetMoney() 
	{
        if (item.isSellable) 
        {
            moneyNode.Visible = true;
            int sellMoney = GameManager.instance.itemManager.GetSellMoney(item);

		    moneyLabel.Text = $"{sellMoney}";
            if(sellMoney < item.money) 
            {
                moneyLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
            }
            else 
            {
                moneyLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
            }
        }
        else 
        {
            moneyNode.Visible = false;
        }
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

            int attackPoint = GameManager.instance.battleManager.GetAttackerPoint(attacker);
            attackPointLabel.Text = $"{attackPoint}";

            if(attackPoint < attacker.attackPoint) 
            {
                attackPointLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
            }
            else if(attackPoint > attacker.attackPoint) 
            {
                attackPointLabel.SelfModulate = GameManager.instance.uiCommonSetting.betterColor;
            }
            else 
            {
                attackPointLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
            }
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

        if (item is ProduceResource produce && produce.nowParameter is IMake make)
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
        if (item is ProduceResource produce && GameManager.instance.itemConfig.config.TryGetValue(produce.nowParameter.productItem, out ItemBaseResource productItem)) 
        {
            productParent.Visible = true;
            productIcon.onMainClick -= OnItemDetailClick;
            productIcon.onMainClick += OnItemDetailClick;
            productIcon.SetData(productItem);

            if (interactable)
            {
                productLeftButton.Disabled = false;
                productRightButton.Disabled = false;
            }
            else
            {
                productLeftButton.Disabled = true;
                productRightButton.Disabled = true;
            }

            if (produce.parameters.Count > 1) 
            {
                productLeftButton.Visible = true;
                productRightButton.Visible = true;
            }
            else 
            {
                productLeftButton.Visible = false;
                productRightButton.Visible = false;
            }
        }
        else 
        {
            productParent.Visible = false;
            productIcon.onMainClick -= OnItemDetailClick;
        }
    }

    private void SetProduceCostTime() 
    {
        if (item is ProduceResource produce)
        {
            produceCostTimeParent.Visible = true;
            double needTime = GameManager.instance.itemManager.GetProduceNeedTime(produce);
            produceCostTimeLabel.Text = $"{needTime}";
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

    private void SetNeedArea() 
    {
        AreaIndex needShowArea = AreaIndex.None;
        if (item is ICore core)
        {
            needShowArea = AreaIndex.Core;
        }
        else if(item is IUseable useable) 
        {
            needShowArea = AreaIndex.Useable;
        }
        else if(item is ProduceResource producer) 
        {
            switch (producer.type) 
            {
                case ProduceType.Fire:
                    needShowArea = AreaIndex.Fire;
                    break;
                case ProduceType.Produce:
                    needShowArea = AreaIndex.Produce;
                    break;
                case ProduceType.None:
                    break;
                default:
                    Debug.PrintWarn($"producer.type未定義, producer.type:{producer.type}");
                    break;
            }
        }


        if(needShowArea != AreaIndex.None) 
        {
            if(GameManager.instance.areaConfig.config.TryGetValue(needShowArea, out AreaResource areaData)) 
            {
                needAreaParent.Visible = true;
                needAreaImage.Texture = areaData.texture;
            }
            else 
            {
                needAreaParent.Visible = false;
                Debug.PrintWarn($"雖然有需要顯示的區域圖但是該區域圖未定義, needShowArea:{needShowArea}");
            }
        }
        else
        {
            needAreaParent.Visible = false;
        }
    }

    private void SetMakeArea() 
    {
        if(item.effectRanges.Count > 0 && item.effectRanges[0] != null) 
        {
            if (GameManager.instance.areaConfig.config.TryGetValue(item.effectRanges[0].type, out AreaResource areaData))
            {
                makeAreaParent.Visible = true;
                makeAreaImage.Texture = areaData.texture;
            }
            else
            {
                makeAreaParent.Visible = false;
                Debug.PrintWarn($"雖然有需要顯示的區域圖但是該區域圖未定義, item.effectRanges[0].type:{item.effectRanges[0].type}");
            }
        }
        else 
        {
            makeAreaParent.Visible = false;
        }
    }

    private void SetKeepProduce() 
    {
        if(item is ProduceResource produce) 
        {
            if(produce.durability == 1)     //只能用1次就不顯示
            {
                keepProduceParent.Visible = false;
            }
            else 
            {
                keepProduceParent.Visible = true;
                if (interactable)
                {
                    keepProduceButton.Disabled = false;
                }
                else
                {
                    keepProduceButton.Disabled = true;
                }
                if (produce.isKeepProduce) 
                {
                    keepProduceButton.Text = Tr("ts_common_Yes");
                    keepProduceButton.ThemeTypeVariation = "Button_Green";
                }
                else 
                {
                    keepProduceButton.Text = Tr("ts_common_No");
                    keepProduceButton.ThemeTypeVariation = "Button_Red";
                }
            }
        }
        else 
        {
            keepProduceParent.Visible = false;
        }
    }

    private void RefreshContainerSize() 
    {
        Size = CustomMinimumSize;
    }

    private void ResetContainerPos() 
    {
        //Vector2 orgin = GlobalPosition;
        Vector2 rightDown = GlobalPosition + Size;

        Rect2 viewRect = GetViewportRect();
        Vector2 fix = new Vector2(Math.Max(rightDown.X - viewRect.Size.X, 0), Math.Max(rightDown.Y - viewRect.Size.Y, 0));
        GlobalPosition = GlobalPosition - fix;

        //Debug.Print($"rightDown:{rightDown}, viewRect Size:{viewRect.Size},fix:{fix}, orgin:{orgin}, GlobalPosition:{GlobalPosition}");
    }

    private void OnDurabilityChange(int durability) 
    {
        SetMoney();
    }

    private void OnParameterIndexChange(int preState, int nextState) 
    {
        SetMaterial();
        SetProduct();
    }

    public void OnKeepProduceButtonClick() 
    {
        if (item is ProduceResource produce)
        {
            produce.isKeepProduce = !produce.isKeepProduce;
        }
        SetKeepProduce();
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
    }

    public void OnItemDetailClick(ItemIcon itemIcon) 
    {
        onDetailClick?.Invoke(index, itemIcon.data);
    }

    public void OnProduceLeftClick() 
    {
        if(item is ProduceResource produce) 
        {
            int result = -1;
            if(0 <= produce.nowParameterIndex -1 && produce.nowParameterIndex -1 < produce.parameters.Count) 
            {
                result = produce.nowParameterIndex - 1;
            }
            else if (produce.parameters.Count != 0)
            {
                result = produce.parameters.Count - 1;
            }
            produce.nowParameterIndex = result;
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button32);
        }
    }

    public void OnProduceRightClick()
    {
        if (item is ProduceResource produce) 
        {
            int result = -1;
            if (0 <= produce.nowParameterIndex + 1 && produce.nowParameterIndex + 1 < produce.parameters.Count)
            {
                result = produce.nowParameterIndex + 1;
            }
            else if (produce.parameters.Count != 0)
            {
                result = 0;
            }
            produce.nowParameterIndex = result;
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button32);
        }
    }

    public void OnCloseClick() 
    {
        onCloseClick?.Invoke(index);
    }
}
