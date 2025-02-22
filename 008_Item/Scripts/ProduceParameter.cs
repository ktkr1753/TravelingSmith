using Godot;
using System;

[GlobalClass]
public partial class ProduceParameter : Resource, IClone<ProduceParameter>, IProduce
{
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

    public virtual ProduceParameter Clone()
    {
        ProduceParameter result = CreateInstanceForClone();

        result.productItem = productItem;
        result.needTime = needTime;

        return result;
    }

    public virtual ProduceParameter CreateInstanceForClone()
    {
        return new ProduceParameter();
    }
}
