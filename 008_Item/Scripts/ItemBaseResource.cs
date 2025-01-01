using Godot;
using System;

[GlobalClass]
public abstract partial class ItemBaseResource : Resource, IClone<ItemBaseResource>
{
    [Export] public ItemIndex index;
    [Export] public Texture2D texture;
    [Export] public int money = 0;
    [Export] public int rank = 0;
    [Export] public bool isSellable = true;

    public virtual ItemBaseResource Clone() 
    {
        ItemBaseResource result = CreateInstanceForClone();
        result.index = index;
        result.texture = texture;
        result.money = money;
        result.rank = rank;
        result.isSellable = isSellable;

        return result;
    }

    public abstract ItemBaseResource CreateInstanceForClone();
}
