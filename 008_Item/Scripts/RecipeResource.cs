using Godot;
using System;

public partial class RecipeResource : ItemBaseResource, IClone<RecipeResource>
{
    public override RecipeResource Clone()
    {
        RecipeResource result = base.Clone() as RecipeResource;

        return result;
    }

    public override RecipeResource CreateInstanceForClone()
    {
        return new RecipeResource();
    }
}
