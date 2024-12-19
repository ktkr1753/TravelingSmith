using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Map : Node2D
{
    [Export] Node2D monsterParent;
    [Export] public Node2D targetPoint;
    [Export] public Node2D roadParent;
    [Export] public PackedScene roadPrefab;
    [Export] public Node2D shopParent;
    [Export] public PackedScene shopPrefab;
    [Export] public double nowTime = 0;
    [Export] public MapItemObjectPool itemPool;
    public List<MonsterObject> monsters = new List<MonsterObject>();

    public const int spawnDistance = 380;

    public int nowWave = 0;


    private Vector2 _viewPortSize = Vector2.Zero;
    public Vector2 viewPortSize 
    {
        get 
        {
            if(_viewPortSize == Vector2.Zero) 
            {
                _viewPortSize = GetViewport().GetVisibleRect().Size;
            }

            return _viewPortSize;
        }
    }
    private Queue<Node2D> roadObjs = new Queue<Node2D>();
    public float targetMoveSpeed = 10;
    private int nowCreateRoadIndex = -1;

    private Queue<ShopObject> shopObjs = new Queue<ShopObject>();
    private int nowCreateShopIndex = -1;
    private int _visitedShopIndex = -1;
    public int visitedShopIndex 
    {
        get { return _visitedShopIndex; }
        private set { _visitedShopIndex = value; }
    }

    public override void _Ready()
    {
        base._Ready();

        CheckRoad();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdateSpawn(delta);
        TargetMove(delta);
        CheckRoad();
        CheckInShop();
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

    public void TargetMove(double delta) 
    {
        double addTime = delta * GameManager.instance.gameSpeed;

        Vector2 moveNormal = new Vector2(1, 0);

        targetPoint.GlobalPosition = targetPoint.GlobalPosition + (moveNormal * (float)(targetMoveSpeed * addTime));
    }

    private void CheckRoad() 
    {
        float needRoadIndex = ((targetPoint.GlobalPosition.X - viewPortSize.X / 2) / viewPortSize.X);
        if(needRoadIndex > nowCreateRoadIndex) 
        {
            nowCreateRoadIndex++;
            Node2D road = UtilityTool.CreateInstance<Node2D>(roadPrefab, roadParent, new Vector2(nowCreateRoadIndex * viewPortSize.X, 0));
            roadObjs.Enqueue(road);

            if (roadObjs.Count > 4) 
            {
                Node2D tempRoad = roadObjs.Dequeue();
                tempRoad.QueueFree();
            }

            if(nowCreateRoadIndex % 3 == 2) 
            {
                nowCreateShopIndex++;
                ShopObject shop = UtilityTool.CreateInstance<ShopObject>(shopPrefab, shopParent, new Vector2(nowCreateRoadIndex * viewPortSize.X + viewPortSize.X / 2, 140));
                shop.SetIndex(nowCreateShopIndex);
                shopObjs.Enqueue(shop);

                if(shopObjs.Count > 1) 
                {
                    ShopObject tempshop = shopObjs.Dequeue();
                    tempshop.QueueFree();
                }
            }
        }
    }

    private void CheckInShop() 
    {
        if(shopObjs.Count > 0) 
        {
            foreach(ShopObject shop in shopObjs) 
            {
                if(shop.index > visitedShopIndex && targetPoint.GlobalPosition.X >= shop.GlobalPosition.X) 
                {
                    visitedShopIndex = shop.index;
                    GameManager.instance.uiManager.OpenUI(UIIndex.ShopUI);
                    break;
                }
            }
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

                float rndAngle = GameManager.instance.randomManager.GetRange(RandomType.SpawnMonster, (float)(-1/2 * Math.PI), (float)(1/2 * Math.PI));
                Vector2 spawnPoint = new Vector2(targetPoint.GlobalPosition.X + spawnDistance * Mathf.Cos(rndAngle), targetPoint.GlobalPosition.X + spawnDistance * Mathf.Sin(rndAngle));

                MonsterObject monster = UtilityTool.CreateInstance<MonsterObject>(monsterData.prefab, monsterParent, spawnPoint);
                monster.SetData(monsterData);
                monster.onDestroy += OnMonsterDestory;

                monsters.Add(monster);
            }
        }
    }

    private void DropMonsterItem(MonsterObject monster)
    {
        if (monster?.data?.drops == null)
        {
            return;
        }

        for (int i = 0; i < monster.data.drops.Count; i++)
        {
            float rnd = GameManager.instance.randomManager.GetRange(RandomType.DropItem, 0f, 1f);
            if (rnd <= monster.data.drops[i].dropRate)
            {
                CreateMapItem(monster.data.drops[i].itemIndex, monster.GlobalPosition);
                break;
            }
        }
    }


    public void CreateMapItem(ItemIndex itemIndex, Vector2 globalPos)
    {
        if (GameManager.instance.itemConfig.config.TryGetValue(itemIndex, out ItemBaseResource item))
        {
            MapItemObject itemObject = itemPool.GetElement();
            itemObject.OnNeedReturn += OnMapItemNeedReturn;
            itemObject.GlobalPosition = globalPos;
            itemObject.SetData(item);
        }
        else
        {
            Debug.PrintWarn($"未定義道具:{itemIndex}");
        }
    }

    public void OnMonsterDestory(MonsterObject monster) 
    {
        monster.onDestroy -= OnMonsterDestory;
        if(this == null || !this.IsExist()) 
        {
            return;
        }
        DropMonsterItem(monster);
        monsters.Remove(monster);
    }

    private void OnMapItemNeedReturn(MapItemObject mapItem) 
    {
        mapItem.OnNeedReturn -= OnMapItemNeedReturn;
        mapItem.SetData(null);
        itemPool.ReturnElement(mapItem);
    }

    private void OnMapItemTouchDeadLine(Area2D area) 
    {
        if (area.Owner is MapItemObject mapItem) 
        {
            //要等到ProcessFrame時才能處理，不然會報錯
            WaitReturnMapItem(mapItem);
        }
    }

    
    private async Task WaitReturnMapItem(MapItemObject mapItem) 
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        OnMapItemNeedReturn(mapItem);
    }
}
