using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class UnlockRecipeResource : Resource, IClone<UnlockRecipeResource>
{
	[Export] public Godot.Collections.Array<UnlockRecipeGroupResource> unlockRecipeOrder = new Godot.Collections.Array<UnlockRecipeGroupResource>();
    private HashSet<ItemIndex> _unlockRecipes = new HashSet<ItemIndex>();

    [Export] public Godot.Collections.Array<ItemIndex> unlockedRecipes 
    {
        get 
        {
            return new Godot.Collections.Array<ItemIndex>(_unlockRecipes);
        }
        set 
        {
            _unlockRecipes = new HashSet<ItemIndex>(value);
        }
    }


    public HashSet<ItemIndex> GetWaitUnlockRecipe() 
    {
        HashSet<ItemIndex> result = new HashSet<ItemIndex>();

        for(int i = 0; i < unlockRecipeOrder.Count; i++) 
        {
            bool isFind = false;
            for(int j = 0; j < unlockRecipeOrder[i].data.Count; j++) 
            {
                if (!_unlockRecipes.Contains(unlockRecipeOrder[i][j])) 
                {
                    result.Add(unlockRecipeOrder[i][j]);
                    isFind = true;
                }
            }

            if (isFind) 
            {
                break;
            }
        }

        return  result;
    }

    public HashSet<ItemIndex> GetUnlockedRecipe() 
    {
        return _unlockRecipes;
    }

    public void AddUnlockRecipe(ItemIndex itemIndex) 
    {
        if (!unlockedRecipes.Contains(itemIndex))
        {
            _unlockRecipes.Add(itemIndex);
        }
    }

    public UnlockRecipeResource Clone()
    {
        UnlockRecipeResource result = CreateInstanceForClone();

        result.unlockRecipeOrder = unlockRecipeOrder.Clone();
        result.unlockedRecipes = unlockedRecipes.Clone();
        return result;
    }

    public UnlockRecipeResource CreateInstanceForClone()
    {
        return new UnlockRecipeResource();
    }
}
