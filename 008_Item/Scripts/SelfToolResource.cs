using Godot;
using System;

[GlobalClass]
public partial class SelfToolResource : ToolResource, IClone<SelfToolResource>, IProduce
{
    public override SelfToolResource Clone()
    {
        SelfToolResource result = base.Clone() as SelfToolResource;
        return result;
    }

    public override SelfToolResource CreateInstanceForClone()
    {
        return new SelfToolResource();
    }
}
