using Godot;
using System;

[GlobalClass]
public abstract partial class ItemBaseResource : Resource, IClone<ItemBaseResource>
{
    [Export] public ItemIndex index;
    [Export] public ItemDetailType detailType;
    [Export] public Texture2D texture;
    [Export] public Godot.Collections.Array<ItemEffect> effectRanges = new Godot.Collections.Array<ItemEffect>();
    [Export] public int money = 0;
    [Export] public int rank = 0;
    [Export] public bool isLowProduct = false;
    [Export] public bool isSellable = true;

    public virtual ItemBaseResource Clone() 
    {
        ItemBaseResource result = CreateInstanceForClone();
        result.index = index;
        result.detailType = detailType;
        result.texture = texture;
        result.effectRanges = effectRanges.Clone();
        result.money = money;
        result.rank = rank;
        result.isLowProduct = isLowProduct;
        result.isSellable = isSellable;

        return result;
    }

    public abstract ItemBaseResource CreateInstanceForClone();
}
