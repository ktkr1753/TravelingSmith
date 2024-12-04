using Godot;
using System;

[GlobalClass]
public partial class RecipeResource : ItemBaseResource, IClone<RecipeResource>, IProduce
{
    [Export] public Godot.Collections.Array<ItemIndex> materials = new Godot.Collections.Array<ItemIndex>();
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

    public override RecipeResource Clone()
    {
        RecipeResource result = base.Clone() as RecipeResource;

        result.materials = materials.Clone();
        result.productItem = productItem;
        result.needTime = needTime;

        return result;
    }

    public override RecipeResource CreateInstanceForClone()
    {
        return new RecipeResource();
    }
}
