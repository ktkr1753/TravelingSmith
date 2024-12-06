using Godot;
using System;
using System.Collections.Generic;

public partial class Map : Node2D
{
	[Export] Godot.Collections.Array<Node2D> spawnPoints = new Godot.Collections.Array<Node2D>();
    [Export] Node2D monsterParent;
    [Export] public Node2D targetPoint;

    [Export] public double nowTime = 0;

    public int nowWave = 0; 

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdateSpawn(delta);
    }


    private void UpdateSpawn(double delta) 
    {
        nowTime = nowTime + delta;

        //測試
        int waveTime = 5;
        if(nowTime / waveTime > nowWave) 
        {
            CreateMonster(MonsterIndex.Slime);
            nowWave = (int)Math.Ceiling(nowTime / waveTime);
        }

    }

    public MonsterObject CreateMonster(MonsterIndex index)
    {
        MonsterObject result = null;
        if (GameManager.instance.monsterConfig.config.TryGetValue(index, out MonsterResource tempMonster))
        {
            MonsterResource monsterData = tempMonster.Clone();
            monsterData.nowHp = monsterData.maxHp;

            int rnd = GameManager.instance.randomManager.GetRange(RandomType.SpawnMonster, 0, spawnPoints.Count);

            if(rnd < spawnPoints.Count) 
            {
                Vector2 spawnPoint = spawnPoints[rnd].GlobalPosition;
                result = UtilityTool.CreateInstance<MonsterObject>(monsterData.prefab, monsterParent, spawnPoint);
                result.SetData(monsterData);
            }
        }

        return result;
    }
}
