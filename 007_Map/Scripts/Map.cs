using Godot;
using System;
using System.Collections.Generic;

public partial class Map : Node2D
{
    [Export] Node2D monsterParent;
    [Export] public Node2D targetPoint;

    [Export] public double nowTime = 0;

    public List<MonsterObject> monsters = new List<MonsterObject>();

    public const int spawnDistance = 380;

    public int nowWave = 0; 

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdateSpawn(delta);
    }


    private void UpdateSpawn(double delta) 
    {
        double addTime = delta * GameManager.instance.gameSpeed;
        nowTime = nowTime + addTime;

        //測試
        int waveTime = 10;
        if(nowTime / waveTime > nowWave) 
        {
            CreateWaveMonster();
            nowWave = (int)Math.Ceiling(nowTime / waveTime);
        }
    }

    public void CreateWaveMonster() 
    {
        if(nowWave < 5) 
        {
            CreateMonster(MonsterIndex.Slime, 1);
        }
        else if (nowWave < 15)
        {
            CreateMonster(MonsterIndex.Slime, 3);
        }
        else if(nowWave < 25) 
        {
            CreateMonster(MonsterIndex.Slime, 3);
            CreateMonster(MonsterIndex.Beholder, 1);
        }
        else 
        {
            CreateMonster(MonsterIndex.Beholder, 3);
        }

    }


    public void CreateMonster(MonsterIndex index, int num = 1)
    {
        for(int i = 0; i < num; i++) 
        {
            if (GameManager.instance.monsterConfig.config.TryGetValue(index, out MonsterResource tempMonster))
            {
                MonsterResource monsterData = tempMonster.Clone();
                monsterData.nowHp = monsterData.maxHp;

                float rndAngle = GameManager.instance.randomManager.GetRange(RandomType.SpawnMonster, 0, (float)(2 * Math.PI));
                Vector2 spawnPoint = new Vector2(targetPoint.GlobalPosition.X + spawnDistance * Mathf.Cos(rndAngle), targetPoint.GlobalPosition.X + spawnDistance * Mathf.Sin(rndAngle));

                MonsterObject monster = UtilityTool.CreateInstance<MonsterObject>(monsterData.prefab, monsterParent, spawnPoint);
                monster.SetData(monsterData);
                monster.onDestroy += OnMonsterDestory;

                monsters.Add(monster);
            }
        }
    }

    public void OnMonsterDestory(MonsterObject monster) 
    {
        monster.onDestroy -= OnMonsterDestory;
        if(this == null || !this.IsExist()) 
        {
            return;
        }
        monsters.Remove(monster);
    }
}
