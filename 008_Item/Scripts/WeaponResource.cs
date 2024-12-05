using Godot;
using System;

[GlobalClass]
public partial class WeaponResource : ItemBaseResource, IClone<WeaponResource>, IUseable
{
    [Export] public int attackPoint = 0;
    private int _durability = 0;
    [Export] public int durability 
    {
        get { return _durability; }
        set { _durability = value; }
    }


    public override WeaponResource Clone()
    {
        WeaponResource result = base.Clone() as WeaponResource;
        result.attackPoint = attackPoint;
        result.durability = durability;

        return result;
    }

    public override WeaponResource CreateInstanceForClone()
    {
        return new WeaponResource();
    }
}
