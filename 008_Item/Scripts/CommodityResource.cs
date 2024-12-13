using Godot;
using System;

[GlobalClass]
public partial class CommodityResource : ItemBaseResource, IClone<CommodityResource>
{
    public override CommodityResource Clone()
    {
        CommodityResource result = base.Clone() as CommodityResource;

        return result;
    }

    public override CommodityResource CreateInstanceForClone()
    {
        return new CommodityResource();
    }
}
