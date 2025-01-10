using Godot;
using System;

[GlobalClass]
public partial class UnlockRecipeGroupResource : Resource,IClone<UnlockRecipeGroupResource>
{
    [Export] public Godot.Collections.Array<ItemIndex> data = new Godot.Collections.Array<ItemIndex>();

    public ItemIndex this[int index] 
    {
        get 
        {
            return data[index];
        }
        set 
        {
            data[index] = value;
        }
    }

    public UnlockRecipeGroupResource Clone()
    {
        UnlockRecipeGroupResource result = CreateInstanceForClone();
        result.data = data.Clone();
        return result;
    }

    public UnlockRecipeGroupResource CreateInstanceForClone()
    {
        return new UnlockRecipeGroupResource();
    }
}
