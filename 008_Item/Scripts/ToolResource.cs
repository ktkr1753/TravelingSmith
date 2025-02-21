using Godot;
using System;

[GlobalClass]
public partial class ToolResource : ItemBaseResource, IClone<ToolResource>, IProduce
{
    private ProduceType _type;
    [Export] public ProduceType type
    {
        get { return _type; }
        set { _type = value; }
    }
    private ItemIndex _productItem = ItemIndex.None;
    [Export] public ItemIndex productItem 
    {
        get { return _productItem; }
        set { _productItem = value; }
    }
    private double _needTime = 0;
    [Export] public double needTime 
    {
        get { return _needTime; }
        set { _needTime = value; }
    }
    private double _nowTime = 0;
    [Export] public double nowTime
    {
        get { return _nowTime; }
        set { _nowTime = value; }
    }

    private int _durability = -1;
    [Export] public int durability 
    {
        get { return _durability; }
        set 
        {
            if (_durability != value) 
            {
                _durability = value;
                onDurabilityChange?.Invoke(_durability);
            }
        }
    }

    [Export] private bool _isKeepProduce = true;
    public bool isKeepProduce {
        get { return _isKeepProduce; }
        set
        {
            if (_isKeepProduce != value)
            {
                _isKeepProduce = value;
            }
        }
    }

    private bool _isProducing = false;
    [Export] public bool isProducing
    {
        get { return _isProducing; }
        private set 
        { 
            _isProducing = value;
            onIsProducingChange?.Invoke(_isProducing);
        }
    }

    public event Action<bool> onIsProducingChange;
    public event Action<int> onDurabilityChange;

    public void StartProduce()
    {
        isProducing = true;
        GameManager.instance.itemManager.AddProducingItem(this);
    }
    public void StopProduce()
    {
        isProducing = false;
        GameManager.instance.itemManager.RemoveProducingItem(this);
    }

    public override ToolResource Clone()
    {
        ToolResource result = base.Clone() as ToolResource;
        result.type = type;
        result.productItem = productItem;
        result.needTime = needTime;
        result.nowTime = nowTime;
        result.durability = durability;
        result.isKeepProduce = isKeepProduce;
        result.isProducing = isProducing;
        return result;
    }

    public override ToolResource CreateInstanceForClone()
    {
        return new ToolResource();
    }
}
