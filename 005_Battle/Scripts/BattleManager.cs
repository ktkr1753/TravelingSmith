using Godot;
using System;

public partial class BattleManager : Node
{
	[Export] public int maxHP;
	[Export] public int nowHP;

	public event Action<int> onHPChange;

	public void Init() 
	{
		nowHP = maxHP;
    }

	public void Damage(int damage) 
	{
		if(damage > 0) 
		{
			nowHP = Math.Max(0, nowHP - damage);
			//後續處理
			onHPChange?.Invoke(nowHP);
        }
	}
}
