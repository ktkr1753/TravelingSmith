using Godot;
using System;

[GlobalClass]
public partial class MaterialResource : ItemBaseResource, IClone<MaterialResource>
{
    public override MaterialResource Clone()
    {
        MaterialResource result = base.Clone() as MaterialResource;

        return result;
    }

    public override MaterialResource CreateInstanceForClone()
    {
        return new MaterialResource();
    }
}
