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
    [Export] public int exp;

    public event Action<int> onHPChange;
    public event Action onDie;

    public void Damage(int damage)
    {
        if (damage > 0)
        {
            nowHp = Math.Max(0, nowHp - damage);
            //後續處理
            onHPChange?.Invoke(nowHp);

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
        result.moveSpeed = moveSpeed;
        result.exp = exp;
        return result;
    }

    public MonsterResource CreateInstanceForClone()
    {
        return new MonsterResource();
    }
}
