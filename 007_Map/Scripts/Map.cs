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
    [Export] public double nowTime = -30;
    [Export] public MapItemObjectPool itemPool;
    public List<MonsterObject> monsters = new List<MonsterObject>();
    public List<MonsterObject> waitRemoveMonsters = new List<MonsterObject>();

    public const int spawnDistance = 1000;

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
    //private float targetMoveSpeed = 50;
    private int nowCreateRoadIndex = -1;

    private Queue<ShopObject> shopObjs = new Queue<ShopObject>();
    private int nowCreateShopIndex = -1;
    private int _visitedShopIndex = -1;
    public int visitedShopIndex 
    {
        get { return _visitedShopIndex; }
        private set { _visitedShopIndex = value; }
    }

    public event Action<MonsterObject> onCreateMonster;

    public override void _Ready()
    {
        base._Ready();

        CheckRoad();
    }

    public void Init() 
    {
        GameManager.instance.battleManager.onHPChange += OnHPChange;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.battleManager.onHPChange -= OnHPChange;
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

    private async void CreateWaveMonster() 
    {
        if(nowWave < 10) 
        {
            await CreateMonster(MonsterIndex.PossessedBook, 1);
        }
        else if (nowWave < 20)
        {
            await CreateMonster(MonsterIndex.Slime, 2);
        }
        else if(nowWave < 30) 
        {
            await CreateMonster(MonsterIndex.Slime, 1);
            await CreateMonster(MonsterIndex.Beholder, 1);
        }
        else 
        {
            await CreateMonster(MonsterIndex.Beholder, 2);
        }
    }

    private void TargetMove(double delta) 
    {
        List<KeyValuePair<double, MonsterObject>> monsters = GameManager.instance.mapManager.FindMonsterInRange(MonsterObject.closeDistance);
        if(monsters.Count == 0)     //附近有怪物不能動
        {
            double addTime = delta * GameManager.instance.gameSpeed;
            Vector2 moveNormal = new Vector2(1, 0);
            targetPoint.GlobalPosition = targetPoint.GlobalPosition + (moveNormal * (float)(GameManager.instance.itemManager.moveSpeed * addTime));
        }
        else 
        {
            int damage = (int)Math.Floor(GameManager.instance.itemManager.moveSpeed / 10);
            //Debug.Print($"TargetMove GameManager.instance.itemManager.moveSpeed:{GameManager.instance.itemManager.moveSpeed},damage:{damage}");
            if(damage > 0) 
            {
                monsters[0].Value.data.Damage(damage);
                GameManager.instance.mapManager.PlayFX(FXEnum.FX1, monsters[0].Value.GlobalPosition);
                GameManager.instance.cameraManager.ShakeCamera();
                GameManager.instance.soundManager.PlaySound(SoundEnum.sound_hit);
            }

            GameManager.instance.itemManager.moveSpeed = 0;
        }
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

            if(nowCreateRoadIndex % 5 == 4) 
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


    public async Task CreateMonster(MonsterIndex index, int num = 1)
    {
        for(int i = 0; i < num; i++) 
        {
            if (GameManager.instance.monsterConfig.config.TryGetValue(index, out MonsterResource tempMonster))
            {
                MonsterResource monsterData = tempMonster.Clone();
                monsterData.nowHp = monsterData.maxHp;

                //float rndAngle = GameManager.instance.randomManager.GetRange(RandomType.SpawnMonster, (float)(-1/2 * Math.PI), (float)(1/2 * Math.PI));
                //float rndAngle = GameManager.instance.randomManager.GetRange(RandomType.SpawnMonster, (float)(-Math.PI), (float)(Math.PI));
                float rndAngle = 0;
                Vector2 spawnPoint = new Vector2(targetPoint.GlobalPosition.X + spawnDistance * Mathf.Cos(rndAngle), targetPoint.GlobalPosition.Y + spawnDistance * Mathf.Sin(rndAngle));

                MonsterObject monster = UtilityTool.CreateInstance<MonsterObject>(monsterData.prefab, monsterParent, spawnPoint);
                monster.SetData(monsterData);
                monster.onDestroy += OnMonsterDestory;
                monster.onDie += OnMonsterDie;

                monsters.Add(monster);

                onCreateMonster?.Invoke(monster);

                await GameManager.instance.Wait(1);
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

    private void ShowBattleHPInfo(int hpChange)
    {
        BattleInfoUI battleInfoUI = GameManager.instance.uiManager.GetOpenedUI<BattleInfoUI>(UIIndex.BattleInfoUI);

        if (hpChange < 0)
        {
            Vector2 viewPos = (GetViewportRect().Size / 2) + (targetPoint.GlobalPosition - GameManager.instance.cameraManager.camera.GlobalPosition);
            Vector2 showPos = new Vector2(viewPos.X, viewPos.Y + -12);
            battleInfoUI.ShowMinusHPInfo(-hpChange, showPos);
        }
    }

    private void CreateMapItem(ItemIndex itemIndex, Vector2 globalPos)
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

    private void OnMonsterDie(MonsterObject monster) 
    {
        monster.onDie -= OnMonsterDie;
        if (this == null || !this.IsExist())
        {
            return;
        }
        DropMonsterItem(monster);
        monsters.Remove(monster);
        waitRemoveMonsters.Add(monster);
    }

    private void OnMonsterDestory(MonsterObject monster) 
    {
        monster.onDestroy -= OnMonsterDestory;
        if(this == null || !this.IsExist()) 
        {
            return;
        }
        waitRemoveMonsters.Remove(monster);
    }

    private void OnMapItemNeedReturn(MapItemObject mapItem) 
    {
        mapItem.OnNeedReturn -= OnMapItemNeedReturn;
        mapItem.SetData(null);
        itemPool.ReturnElement(mapItem);
    }

    private void OnMapItemTouchDeadLine(Area2D area) 
    {
        //要等到ProcessFrame時才能處理，不然會報錯
        if (area.Owner is MapItemObject mapItem) 
        {
            WaitReturnMapItem(mapItem);
        }
        else if(area.Owner is MonsterObject monster) 
        {
            WaitQueueFreeMonster(monster);
        }
    }

    
    private async void WaitReturnMapItem(MapItemObject mapItem) 
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        OnMapItemNeedReturn(mapItem);
    }

    private async void WaitQueueFreeMonster(MonsterObject monster)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        monster.QueueFree();
    }

    private void OnHPChange(int preHP, int nowHP) 
    {
        ShowBattleHPInfo(nowHP - preHP);
    }
}
