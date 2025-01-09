using Godot;
using System;

[GlobalClass]
public partial class RecipeResource : ItemBaseResource, IClone<RecipeResource>, IMake
{
    private MakeType _type;
    [Export] public MakeType type 
    {
        get { return _type; }
        set { _type = value; }
    }


    private Godot.Collections.Array<ItemIndex> _materials = new Godot.Collections.Array<ItemIndex>();

    [Export] public Godot.Collections.Array<ItemIndex> materials 
    {
        get { return _materials; }
        private set { _materials = value; }
    }

    private bool _isCostMaterial = false;
    [Export] public bool isCostMaterial 
    {
        get { return _isCostMaterial; }
        set { _isCostMaterial = value; }
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

    public bool isKeepProduce { get { return false; } }

    private bool _isProducing = false;
    [Export] public bool isProducing
    {
        get { return _isProducing; }
        private set 
        { 
            if(_isProducing != value) 
            {
                _isProducing = value;
                onIsProducingChange?.Invoke(_isProducing);
            }
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

    public override RecipeResource Clone()
    {
        RecipeResource result = base.Clone() as RecipeResource;
        result.type = type;
        result.materials = materials.Clone();
        result.durability = durability;
        result.productItem = productItem;
        result.needTime = needTime;
        result.nowTime = nowTime;
        result.isProducing = isProducing;
        return result;
    }

    public override RecipeResource CreateInstanceForClone()
    {
        return new RecipeResource();
    }
}
