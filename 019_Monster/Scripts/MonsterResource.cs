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
    [Export] public string sound;
    [Export] public double moveSpeed;
    [Export] public int exp;
    [Export] public Godot.Collections.Array<DropItemResource> drops = new Godot.Collections.Array<DropItemResource>();

    public event Action<int, int> onHPChange;
    public event Action onDie;

    public void Damage(int damage)
    {
        if (damage > 0)
        {
            int preHp = nowHp;
            nowHp = Math.Max(0, nowHp - damage);
            //後續處理
            onHPChange?.Invoke(preHp, nowHp);

            if (nowHp == 0)
            {
                Die();
                onDie?.Invoke();
            }
        }
    }

    public void Die() 
    {
        GameManager.instance.battleManager.nowExp += exp;
    }

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
        result.sound = sound;
        result.moveSpeed = moveSpeed;
        result.exp = exp;
        result.drops = drops;
        return result;
    }

    public MonsterResource CreateInstanceForClone()
    {
        return new MonsterResource();
    }
}
