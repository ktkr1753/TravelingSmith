using Godot;
using System;

[GlobalClass]
public partial class RecipeResource : ItemBaseResource, IClone<RecipeResource>, IMake
{
    private Godot.Collections.Array<ItemIndex> _materials = new Godot.Collections.Array<ItemIndex>();

    [Export] public Godot.Collections.Array<ItemIndex> materials 
    {
        get { return _materials; }
        private set { _materials = value; }
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

        result.materials = materials.Clone();
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
