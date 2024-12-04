using Godot;
using System;

[GlobalClass]
public partial class ToolResource : ItemBaseResource, IClone<ToolResource>, IProduce
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

    public override ToolResource Clone()
    {
        ToolResource result = base.Clone() as ToolResource;
        result.productItem = productItem;
        result.needTime = needTime;
        return result;
    }

    public override ToolResource CreateInstanceForClone()
    {
        return new ToolResource();
    }
}
