using Godot;
using System;

[GlobalClass]
public partial class ItemEffect : Resource, IClone<ItemEffect>
{
	[Export] public Vector2I position;
	[Export] public AreaIndex type;

    public ItemEffect Clone()
    {
        ItemEffect result = CreateInstanceForClone();

        result.position = position;
        result.type = type;

        return result;
    }

    public ItemEffect CreateInstanceForClone()
    {
        return new ItemEffect();
    }

    public bool IsSame(ItemEffect effect) 
    {
        bool result = false;

        if(position == effect.position && type == effect.type) 
        {
            result = true;
        }

        return result;
    }

    public override string ToString()
    {
        return $"position:{position}, type:{type}";
    }
}
