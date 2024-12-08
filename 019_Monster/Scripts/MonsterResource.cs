using Godot;
using System;

[GlobalClass]
public partial class MonsterResource : Resource, IClone<MonsterResource>
{
	[Export] public MonsterIndex index;
    [Export] public PackedScene prefab;
	[Export] public int maxHp;
	[Export] public int nowHp;
	[Export] public int attackPoint;
    [Export] public double attackNeedTime;
    [Export] public double attackNowTime;
    [Export] public FXEnum attackFX;
    [Export] public double moveSpeed;


    public MonsterResource Clone()
    {
        MonsterResource result = CreateInstanceForClone();
        result.index = index;
        result.prefab = prefab;
        result.maxHp = maxHp;
        result.nowHp = nowHp;
        result.attackPoint = attackPoint;
        result.attackNeedTime = attackNeedTime;
        result.attackNowTime = attackNowTime;
        result.attackFX = attackFX;
        result.moveSpeed = moveSpeed;
        return result;
    }

    public MonsterResource CreateInstanceForClone()
    {
        return new MonsterResource();
    }
}
