using Godot;
using System;

[GlobalClass]
public abstract partial class ItemBaseResource : Resource, IClone<ItemBaseResource>
{
    [Export] public ItemIndex index;
    [Export] public int money = 0;
    [Export] public int rank = 0;

    public virtual ItemBaseResource Clone() 
    {
        ItemBaseResource result = CreateInstanceForClone();

        return result;
    }

    public abstract ItemBaseResource CreateInstanceForClone();
}
