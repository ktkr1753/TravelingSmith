using Godot;
using System;

[GlobalClass]
public partial class RangeResource : ItemBaseResource, IClone<RangeResource>
{
    public override RangeResource Clone()
    {
        RangeResource result = base.Clone() as RangeResource;

        return result;
    }

    public override RangeResource CreateInstanceForClone()
    {
        return new RangeResource();
    }
}
