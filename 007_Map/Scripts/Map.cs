using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Map : Node2D
{
    [Export] public Node2D monsterParent;
    [Export] public MapMain main;
    [Export] public Node2D roadParent;
    [Export] public PackedScene roadPrefab;
    [Export] public PackedScene roadShopPrefab;
    [Export] public Node2D shopParent;
    [Export] public PackedScene shopPrefab;
    [Export] public Node2D attackersParent;
    [Export] public MapItemObjectPool itemPool;
    [Export] public Godot.Collections.Array<DropItemResource> mapDropItems = new Godot.Collections.Array<DropItemResource>();
    public List<MonsterObject> monsters = new List<MonsterObject>();
    public List<MonsterObject> waitRemoveMonsters = new List<MonsterObject>();

    public double nowTime = -50;
    public double waveTime = 50;
    public const int spawnDistance = 500;
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
    private int nowCreateRoadIndex = -1;

    private Queue<ShopObject> shopObjs = new Queue<ShopObject>();
    private int nowCreateShopIndex = -1;
    private int _visitedShopIndex = -1;
    public int visitedShopIndex 
    {
        get { return _visitedShopIndex; }
        private set { _visitedShopIndex = value; }
    }

    private double createItemMinDistance = 320;
    private double createItemDistance = 64;
    private double createItemCount = 0;

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

        //UpdateSpawn(delta);
        TargetMove(delta);
        CheckRoad();
        CheckInShop();
        CheckCreateRandomMapItem();
    }


    private void UpdateSpawn(double delta) 
    {
        double addTime = delta * GameManager.instance.gameSpeed;
        nowTime = nowTime + addTime;

        //測試
        if(nowTime / waveTime > nowWave) 
        {
            CreateWaveMonster();
            nowWave = (int)Math.Ceiling(nowTime / waveTime);
        }
    }

    private async void CreateWaveMonster() 
    {
        if(nowWave < 5) 
        {
            await CreateMonster(MonsterIndex.Slime, 1);
        }
        else if (nowWave < 10)
        {
            await CreateMonster(MonsterIndex.Slime, 1);
            await CreateMonster(MonsterIndex.VampireBat, 1);
        }
        else if (nowWave < 15)
        {
            await CreateMonster(MonsterIndex.Slime, 1);
            await CreateMonster(MonsterIndex.VampireBat, 2);
        }
        else if(nowWave < 20) 
        {
            await CreateMonster(MonsterIndex.Slime, 1);
            await CreateMonster(MonsterIndex.VampireBat, 1);
            await CreateMonster(MonsterIndex.PossessedBook, 1);
        }
        else 
        {
            await CreateMonster(MonsterIndex.VampireBat, 2);
            await CreateMonster(MonsterIndex.PossessedBook, 2);
        }
    }

    private void TargetMove(double delta) 
    {
        List<KeyValuePair<double, MonsterObject>> monsters = GameManager.instance.mapManager.FindMonsterInRange(MonsterObject.closeDistance);
        if(monsters.Count == 0)     //附近有怪物不能動
        {
            GameManager.instance.itemManager.isTouch = false;
            double addTime = delta * GameManager.instance.gameSpeed;
            Vector2 moveNormal = new Vector2(1, 0);
            main.GlobalPosition = main.GlobalPosition + (moveNormal * (float)(GameManager.instance.itemManager.moveSpeed * addTime));
        }
        else 
        {
            int damage = (int)Math.Floor((GameManager.instance.itemManager.moveSpeed - 10) / 20);
            //Debug.Print($"TargetMove GameManager.instance.itemManager.moveSpeed:{GameManager.instance.itemManager.moveSpeed},damage:{damage}");
            if(damage > 0) 
            {
                monsters[0].Value.data.Damage(damage, HPChangeType.Crash);
                GameManager.instance.mapManager.PlayFX(FXEnum.FX1, monsters[0].Value.GlobalPosition);
                GameManager.instance.cameraManager.ShakeCamera();
                GameManager.instance.soundManager.PlaySound(SoundEnum.sound_hit);
            }

            //GameManager.instance.itemManager.moveSpeed = 0;

            GameManager.instance.itemManager.isTouch = true;
            double addTime = delta * GameManager.instance.gameSpeed;
            Vector2 moveNormal = new Vector2(1, 0);
            main.GlobalPosition = main.GlobalPosition + (moveNormal * (float)(GameManager.instance.itemManager.moveSpeed * addTime));
        }
    }


    private void CheckRoad() 
    {
        float needRoadIndex = ((main.GlobalPosition.X - viewPortSize.X / 2) / viewPortSize.X);
        if(needRoadIndex > nowCreateRoadIndex) 
        {
            nowCreateRoadIndex++;
            MapRoadObject road = null;

            if (nowCreateRoadIndex % 3 == 2) //生成商店
            {
                road = UtilityTool.CreateInstance<MapRoadObject>(roadShopPrefab, roadParent, new Vector2(nowCreateRoadIndex * viewPortSize.X, 0));

                nowCreateShopIndex++;
                ShopObject shop = UtilityTool.CreateInstance<ShopObject>(shopPrefab, shopParent, new Vector2(road.shopNode.GlobalPosition.X, road.shopNode.GlobalPosition.Y));
                shop.SetIndex(nowCreateShopIndex);
                shopObjs.Enqueue(shop);

                if(shopObjs.Count > 1) 
                {
                    ShopObject tempshop = shopObjs.Dequeue();
                    tempshop.QueueFree();
                }
            }
            else 
            {
                road = UtilityTool.CreateInstance<MapRoadObject>(roadPrefab, roadParent, new Vector2(nowCreateRoadIndex * viewPortSize.X, 0));
            }

            if (nowCreateRoadIndex % 4 == 3)    //生成怪物
            {
                CreateWaveMonster();
                nowWave++;
            }


            roadObjs.Enqueue(road);
            if (roadObjs.Count > 4)
            {
                Node2D tempRoad = roadObjs.Dequeue();
                tempRoad.QueueFree();
            }
        }
    }

    private void CheckInShop() 
    {
        if(shopObjs.Count > 0) 
        {
            foreach(ShopObject shop in shopObjs) 
            {
                if(shop.index > visitedShopIndex)
                {
                    if (main.GlobalPosition.X >= shop.GlobalPosition.X)
                    {
                        visitedShopIndex = shop.index;
                        GameManager.instance.uiManager.OpenUI(UIIndex.ShopUI);
                        break;
                    }
                    else if (main.GlobalPosition.X >= shop.enterShopNode.GlobalPosition.X && shop.nowCharacterState == ShopObject.CharacterState.Out)
                    {
                        shop.nowCharacterState = ShopObject.CharacterState.In;
                    }
                }
                else if(shop.index == visitedShopIndex) 
                {
                    if(main.GlobalPosition.X >= shop.exitShopNode.GlobalPosition.X && shop.nowCharacterState == ShopObject.CharacterState.In)
                    {
                        shop.nowCharacterState = ShopObject.CharacterState.Out;
                    }
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

                float rndFixedX = GameManager.instance.randomManager.GetRange(RandomType.SpawnMonster, -16, 17);
                float rndFixedY = GameManager.instance.randomManager.GetRange(RandomType.SpawnMonster, -16, 17);
                Vector2 spawnPoint = new Vector2(main.GlobalPosition.X + spawnDistance + rndFixedX, main.GlobalPosition.Y + rndFixedY);

                MonsterObject monster = UtilityTool.CreateInstance<MonsterObject>(monsterData.prefab, monsterParent, spawnPoint);
                monster.SetData(monsterData);
                monster.onDestroy += OnMonsterDestory;
                monster.onDie += OnMonsterDie;

                monsters.Add(monster);

                onCreateMonster?.Invoke(monster);

                //await GameManager.instance.Wait(0.5f);
            }
            else 
            {
                Debug.PrintWarn($"未找到該怪物,index:{index}");
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
                MapItemObject itemObj = CreateMapItem(monster.data.drops[i].itemIndex, monster.GlobalPosition);
                Tween tween = CreateTween();

                float rndDis = 30 * GameManager.instance.randomManager.GetRange(RandomType.Other, 0.5f, 1);
                float rndAngle = GameManager.instance.randomManager.GetRange(RandomType.Other, -0.5f * Mathf.Pi, 0.5f * Mathf.Pi);
                Vector2 rndMove = new Vector2(rndDis * (float)Math.Cos(rndAngle), rndDis * (float)Math.Sin(rndAngle));
                tween.SetEase(Tween.EaseType.Out);
                tween.TweenProperty(itemObj, "global_position", itemObj.GlobalPosition + rndMove, 0.2f) ;
                break;
            }
        }
    }

    private void ShowBattleHPInfo(int hpChange, HPChangeType type)
    {
        BattleInfoUI battleInfoUI = GameManager.instance.uiManager.GetOpenedUI<BattleInfoUI>(UIIndex.BattleInfoUI);

        if (hpChange < 0)
        {
            Vector2 viewPos = (GetViewportRect().Size / 2) + (main.GlobalPosition - GameManager.instance.cameraManager.camera.GlobalPosition);
            Vector2 showPos = new Vector2(viewPos.X, viewPos.Y + -12);
            battleInfoUI.ShowMinusHPInfo(-hpChange, showPos, type);
        }
    }

    private void CheckCreateRandomMapItem() 
    {
        while(((nowCreateRoadIndex + 1) * 640 - createItemMinDistance) / createItemDistance > createItemCount)
        {
            Queue<int> rndYFixGrid = new Queue<int>(GameManager.instance.randomManager.GetNotRepeatList(RandomType.Other, -1, 4));
            for (int i = 0; i < mapDropItems.Count; i++) 
            {
                float rnd = GameManager.instance.randomManager.GetRange(RandomType.DropItem, 0f, 1f);
                if (rnd <= mapDropItems[i].dropRate && rndYFixGrid.Count > 0) 
                {
                    int rndYFix = rndYFixGrid.Dequeue() * 16;
                    Vector2 itemGlobalPos = new Vector2((float)(createItemMinDistance + createItemDistance * (createItemCount + 1)), main.GlobalPosition.Y + rndYFix);
                    MapItemObject itemObj = CreateMapItem(mapDropItems[i].itemIndex, itemGlobalPos);
                }
            }
            createItemCount++;
        }
    }

    private MapItemObject CreateMapItem(ItemIndex itemIndex, Vector2 globalPos)
    {
        MapItemObject itemObject = null;
        if (GameManager.instance.itemConfig.config.TryGetValue(itemIndex, out ItemBaseResource item))
        {
            itemObject = itemPool.GetElement();
            itemObject.OnNeedReturn -= OnMapItemNeedReturn;
            itemObject.OnNeedReturn += OnMapItemNeedReturn;
            itemObject.GlobalPosition = globalPos;
            itemObject.SetData(item);
        }
        else
        {
            Debug.PrintWarn($"未定義道具:{itemIndex}");
        }

        return itemObject;
    }

    public MapAttackObject CreateMapAttack(ItemIndex itemIndex, Vector2 globalPos)
    {
        MapAttackObject attackObject = null;

        if(GameManager.instance.mapAttackConfig.config.TryGetValue(itemIndex, out PackedScene mapAttackPrefab)) 
        {
            attackObject = UtilityTool.CreateInstance<MapAttackObject>(mapAttackPrefab, attackersParent);

            if(attackObject.isNear) 
            {
                attackObject.GlobalPosition = globalPos + new Vector2(0, -8);
            }
            else 
            {
                attackObject.GlobalPosition = main.GlobalPosition + new Vector2(0, -8);

                double angle = Math.Atan2(globalPos.Y - attackObject.GlobalPosition.Y, globalPos.X - attackObject.GlobalPosition.X);
                //Debug.Print($"CreateMapAttack angle:{angle}");
                attackObject.SetAngle(angle);
            }
        }
        else 
        {
            Debug.PrintWarn($"未定義攻擊物件");
        }

        return attackObject;
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
        //Debug.Print($"OnMapItemNeedReturn mapItem:{mapItem.item.index}, itemGlobalPos:{mapItem.GlobalPosition}");
        mapItem.OnNeedReturn -= OnMapItemNeedReturn;
        mapItem.SetData(null);
        itemPool.ReturnElement(mapItem);
    }

    private void OnMapItemTouchDeadLine(Area2D area) 
    {
        //要等到ProcessFrame時才能處理，不然會報錯
        if (area.Owner is MapItemObject mapItem && !mapItem.isTouched) 
        {
            mapItem.isTouched = true;
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

    private void OnHPChange(int preHP, int nowHP, HPChangeType type) 
    {
        ShowBattleHPInfo(nowHP - preHP, type);
    }
}
