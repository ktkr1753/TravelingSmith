using Godot;
using System;
using System.Collections.Generic;

public partial class ItemManager : Node
{
    private int _money = 0;
    [Export] public int money 
    {
        get { return _money; }
        set 
        { 
            if(_money != value) 
            {
                _money = value;
                onMoneyChange?.Invoke(_money);
            }
        }
    }

    private double _moveAcceleration = 0;
    [Export] public double moveAcceleration
    {
        get { return _moveAcceleration; }
        set
        {
            if (_moveAcceleration != value)
            {
                _moveAcceleration = value;
                onMoveAccelerationChange?.Invoke(_moveAcceleration);
            }
        }
    }

    public const double brakeAcceleration = -10;
    public const double crashAcceleration = -50;
    public const double crashMaxSpeed = 10;
    public bool _isTouch = false;

    public bool isTouch 
    {
        get { return _isTouch; }
        set 
        {
            if( _isTouch != value) 
            {
                _isTouch = value;
                if (_isTouch) 
                {
                    moveSpeed =  Math.Clamp(moveSpeed, 0, crashMaxSpeed);
                }
            }
        }
    }

    private double _moveSpeed = 0;
    [Export] public double moveSpeed 
    {
        get { return _moveSpeed; }
        set
        {
            if (_moveSpeed != value)
            {
                _moveSpeed = value;
                onMoveSpeedChange?.Invoke(_moveSpeed);
            }
        }
    }
    private Godot.Collections.Array<FeatureIndex> _haveFeatures = new Godot.Collections.Array<FeatureIndex>();
    [Export] public Godot.Collections.Array<FeatureIndex> haveFeatures 
    {
        get { return _haveFeatures; }
        private set {
            _haveFeatures = value;
        }
    }

    private Godot.Collections.Array<ItemBaseResource> _heldItems = new Godot.Collections.Array<ItemBaseResource>();
    [Export] public Godot.Collections.Array<ItemBaseResource> heldItems 
	{
		get { return _heldItems; }
		private set { _heldItems = value; }
	}

    private Godot.Collections.Array<AreaIndex> _defaultAreas = new Godot.Collections.Array<AreaIndex>();
    [Export] public Godot.Collections.Array<AreaIndex> defaultAreas
    {
        get { return _defaultAreas; }
        private set { _defaultAreas = value; }
    }

    private Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> _itemEffects = new Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>>();

    [Export] public Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> itemEffects 
    {
        get { return _itemEffects; }
        private set { _itemEffects = value; }
    }

    private bool isAreasDirt = false;

    private Godot.Collections.Array<AreaIndex> _areas = new Godot.Collections.Array<AreaIndex>();

    [Export] public Godot.Collections.Array<AreaIndex> areas 
    {
        get {
            if (isAreasDirt) 
            {
                RefreshAreas(_areas, itemEffects);
                isAreasDirt = false;
            }
            return _areas;
        }
        private set { _areas = value; }
    }

    public bool isHeldItemFull 
    {
        get 
        {
            bool isAnyEmpty = false;

            for(int i = 0; i < _heldItems.Count; i++) 
            {
                if (_heldItems[i] == null && IsAreaEffect(areas, i, AreaIndex.Normal)) 
                {
                    isAnyEmpty = true;
                }
            }
            return !isAnyEmpty;
        }
    }

    HashSet<ItemBaseResource> producingItems = new HashSet<ItemBaseResource>();
    HashSet<ItemBaseResource> attackingItems = new HashSet<ItemBaseResource>();
    HashSet<ItemBaseResource> repairingItems = new HashSet<ItemBaseResource>();
    HashSet<ItemBaseResource> coreingItems = new HashSet<ItemBaseResource>();

    public event Action<int> onMoneyChange;
    public event Action<double> onMoveAccelerationChange;
    public event Action<double> onMoveSpeedChange;
    public event Action<int> onHeldItemChange;
    public event Action<int> onItemEffectsChange;
    public event Action<IMake, HashSet<KeyValuePair<int, ItemBaseResource>>> onUseMaterial;         //<make, <usedItemIndexs, usedItem>>
    public event Action<IProduce, int, ItemBaseResource> onCreateProduct;       //<produce, productItemIndex, productItem>


    public const int itemNum = 105;
    public const int itemColumnNum = 15;
    public int itemLineNum 
    {
        get 
        {
            return itemNum / itemColumnNum;
        }
    }

    public void Init() 
	{
        RefreshAreas(_areas, itemEffects);
        isAreasDirt = false;
        for (int i = 0; i < itemNum; i++) 
        {
            heldItems.Add(null);
        }

		//測試用道具
        for (int i = 0; i < itemNum; i++)
        {
            ItemBaseResource item = null;
            if (i == 22)
            {
                item = GameManager.instance.itemManager.CreateItem(ItemIndex.StonePickaxe);
            }
            if (i == 52)
            {
                item = GameManager.instance.itemManager.CreateItem(ItemIndex.WoodenWheel);
            }

            SetHeldItem(i, item);
        }
    }

    public int AddHeldItem(ItemIndex itemIndex) 
    {
        int index = -1;

        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] == null && IsAreaEffect(areas, i, AreaIndex.Normal))
            {
                ItemBaseResource item = CreateItem(itemIndex);
                SetHeldItem(i, item);
                index = i;
                break;
            }
        }
        return index;
    }

    public void RemoveItem(int index) 
    {
        if (index >= 0 && index < heldItems.Count)
        {
            ItemBaseResource item = heldItems[index];
            if (producingItems.Contains(item)) 
            {
                producingItems.Remove(item);
            }
            if (attackingItems.Contains(item)) 
            {
                attackingItems.Remove(item);
            }
            if (repairingItems.Contains(item)) 
            {
                repairingItems.Remove(item);
            }
            if (coreingItems.Contains(item))
            {
                coreingItems.Remove(item);
            }
            SetHeldItem(index, null);
        }
    }

	public void SetHeldItem(int index, ItemBaseResource item) 
	{
        if (index >= 0 && index < heldItems.Count) 
		{
            HashSet<int> itemEffectRangeChanges = new HashSet<int>();
            if (heldItems[index] != null) 
            {
                RemoveItemEffects(itemEffects, index, heldItems[index].effectRanges);
                GetItemEffectRangePositions(index, ref itemEffectRangeChanges);
            }
            heldItems[index] = item;
            if (item != null)
            {
                AddItemEffects(itemEffects, index, item.effectRanges);
                GetItemEffectRangePositions(index, ref itemEffectRangeChanges);
            }
			onHeldItemChange?.Invoke(index);
            

            foreach(int effectChangeIndex in itemEffectRangeChanges) 
            {
                onItemEffectsChange?.Invoke(effectChangeIndex);
            }
        }
    }

	public override void _Process(double delta)
	{
        base._Process(delta);
        ProcessProducingItems(delta);
        ProcessAttackingItems(delta);
        ProcessRepairingItems(delta);
        ProcessCoreingItems(delta);
    }

    public ItemBaseResource CreateItem(ItemIndex index) 
	{
		ItemBaseResource result = null;

		if(GameManager.instance.itemConfig.config.TryGetValue(index, out ItemBaseResource tempItem)) 
		{
			result = tempItem.Clone();
        }
		return result;
	}

    public bool SellItem(ItemBaseResource item)
    {
        bool isSuccess = false;
        for(int i = 0; i < heldItems.Count; i++) 
        {
            if (heldItems[i] == item) 
            {
                isSuccess = SellItem(i);
                break;
            }
        }
        return isSuccess;
    }

    public bool SellItem(int index) 
    {
        bool isSuccess = false;
        if (index >= 0 && index < heldItems.Count && heldItems[index] != null) 
        {
            money += GetSellMoney(heldItems[index]);
            GameManager.instance.itemManager.RemoveItem(index);
            isSuccess = true;
        }
        return isSuccess;
    }

    public bool RefreshItem(ItemBaseResource item, int refreshCost) 
    {
        bool isSuccess = false;

        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] == item)
            {
                isSuccess = RefreshItem(i, refreshCost);
                break;
            }
        }

        return isSuccess;
    }

    public bool RefreshItem(int index, int refreshCost) 
    {
        bool isSuccess = false;
        if (GameManager.instance.itemManager.money >= refreshCost)
        {
            if (index >= 0 && index < heldItems.Count && heldItems[index] != null &&
                heldItems[index].index == ItemIndex.Paper || heldItems[index].index == ItemIndex.Parchment)
            {
                List<ItemBaseResource> canBeRandomItems = new List<ItemBaseResource>();
                HashSet<ItemIndex> waitUnlockRecipe = GameManager.instance.unlockRecipe.GetWaitUnlockRecipe();
                HashSet<ItemIndex> unlockedRecipe = GameManager.instance.unlockRecipe.GetUnlockedRecipe();

                Action getWaitUnlockRandom = () =>
                {
                    foreach (var KV in GameManager.instance.itemConfig.config)
                    {
                        if (waitUnlockRecipe.Contains(KV.Value.index))
                        {
                            canBeRandomItems.Add(KV.Value);
                        }
                    }
                };
                Action getUnlockedRandom = () =>
                {
                    foreach (var KV in GameManager.instance.itemConfig.config)
                    {
                        if (unlockedRecipe.Contains(KV.Value.index))
                        {
                            canBeRandomItems.Add(KV.Value);
                        }
                    }
                };

                if(waitUnlockRecipe.Count > 0 && unlockedRecipe.Count > 0) 
                {
                    float rndUnlock = GameManager.instance.randomManager.GetRange(RandomType.RandomItem, 0f, 1f);
                    if (rndUnlock < 2)
                    {
                        getWaitUnlockRandom();
                        //Debug.Print("1");
                    }
                    else //暫時不刷出已解鎖
                    {
                        getUnlockedRandom();
                        //Debug.Print("2");
                    }
                }
                else 
                {
                    if(waitUnlockRecipe.Count > 0) 
                    {
                        getWaitUnlockRandom();
                        //Debug.Print("3");
                    }
                    else if(unlockedRecipe.Count > 0) 
                    {
                        getUnlockedRandom();
                        //Debug.Print("4");
                    }
                    else 
                    {
                        Debug.PrintErr($"未有任何可用配方");
                    }
                }

                int rnd = GameManager.instance.randomManager.GetRange(RandomType.RandomItem, 0, canBeRandomItems.Count);
                ItemBaseResource randomItem = canBeRandomItems[rnd].Clone();
                GameManager.instance.unlockRecipe.AddUnlockRecipe(randomItem.index);
                GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - refreshCost);
                GameManager.instance.itemManager.RemoveItem(index);
                GameManager.instance.itemManager.SetHeldItem(index, randomItem);
                isSuccess = true;
            }
        }

        return isSuccess;
    }

    public bool TryMake(IMake make) 
    {
        return TryMake(make, out HashSet<int> usedItemsIndex);
    }

	public bool TryMake(IMake make, out HashSet<int> usedItemsIndex, Godot.Collections.Array<ItemElement> itemElements = null) 
	{
        bool isFail = false;
        usedItemsIndex = new HashSet<int>();

        for (int i = 0; i < make.materials.Count; i++)
        {
            bool isFind = false;
            for (int j = 0; j < GameManager.instance.itemManager.heldItems.Count; j++)
            {
                if (usedItemsIndex.Contains(j))
                {
                    continue;
                }
                else if (make.materials[i] == GameManager.instance.itemManager.heldItems[j]?.index 
                    && (itemElements == null || !itemElements[j].isFlying)) //視覺物件正在飛的話就不拿來用
                {
                    usedItemsIndex.Add(j);
                    isFind = true;
                    break;
                }
            }

            if (!isFind)
            {
                isFail = true;
                break;
            }
        }

        return !isFail;
    }

    public bool Make(IMake make, Godot.Collections.Array<ItemElement> itemElements = null) 
    {
        bool result = false;
        if(TryMake(make, out HashSet<int> usedItemsIndex, itemElements) && !make.isCostMaterial) 
        {
            HashSet<KeyValuePair<int, ItemBaseResource>> items = new HashSet<KeyValuePair<int, ItemBaseResource>>();
            foreach (int index in usedItemsIndex)
            {
                items.Add(new KeyValuePair<int, ItemBaseResource>(index, heldItems[index]));
                RemoveItem(index);
            }

            make.isCostMaterial = true;
            onUseMaterial?.Invoke(make, items);
            result = true;
        }
        return result;
    }

	public void AddProducingItem(ItemBaseResource item) 
	{
		if(item is IProduce && !producingItems.Contains(item)) 
		{
			producingItems.Add(item);
        }
	}

    public void RemoveProducingItem(ItemBaseResource item)
    {
        if (producingItems.Contains(item)) 
		{
            producingItems.Remove(item);
		}
    }

    private void ProcessProducingItems(double deltaTime) 
	{
        double addTime = deltaTime * GameManager.instance.gameSpeed;
        if(addTime == 0) 
        {
            return;
        }
        List<ItemBaseResource> needRemoves = new List<ItemBaseResource>();

		foreach(var producingItem in producingItems) 
		{
			if (producingItem is IProduce produce)
			{
				if(produce.nowTime + addTime >= produce.needTime) 
				{
                    bool isSuccess = CreateProduct(produce);
                    if (isSuccess) 
                    {
					    if (produce.isKeepProduce) 
					    {
                            produce.nowTime = (produce.nowTime + addTime) % produce.needTime;
                        }
					    else 
					    {
						    produce.StopProduce();
						    produce.nowTime = 0;
                        }

                        if(producingItem is IMake make) 
                        {
                            make.isCostMaterial = false;
                        }
                    }
                    else //物品太多生產失敗，不做事 
                    {
                        produce.nowTime = produce.needTime;
                    }
                }
				else 
				{
	                produce.nowTime = (produce.nowTime + addTime) % produce.needTime;
				}
            }
			else 
			{
				Debug.PrintErr($"不是IProduce, producingItems index:{producingItem.index}");
                needRemoves.Add(producingItem);
            }
		}

		for(int i = 0; i < needRemoves.Count; i++) 
		{
			producingItems.Remove(needRemoves[i]);
        }
	}

    public bool CreateProduct(IProduce produce)
    {
        bool isSuccess = false;

        int refPosition = -1;
        for (int i = 0; i < heldItems.Count; i++) 
        {
            if (heldItems[i] == produce) 
            {
                refPosition = i;
            }
        }

        if(refPosition == -1) 
        {
            return isSuccess;
        }

        List<int> checkPoints = GetPointAroundPoints(refPosition);
        for (int i = 0; i < checkPoints.Count; i++)
        {
            int checkPoint = checkPoints[i];
            if ((heldItems[checkPoint] == null || (heldItems[checkPoint] == produce && produce.durability == 1)) && IsAreaEffect(areas, checkPoint, AreaIndex.Normal))
            {
                ItemBaseResource item = CreateItem(produce.productItem);
                SetHeldItem(checkPoint, item);
                GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble);
                isSuccess = true;

                //有使用次數限制
                if (produce.durability > 0)
                {
                    produce.durability = Math.Max(0, produce.durability - 1);
                    if(produce.durability <= 0) 
                    {
                        produce.StopProduce();
                    }
                }
                onCreateProduct?.Invoke(produce, checkPoint, item);
                break;
            }
        }

        if (produce.durability == 0)
        {
            for (int i = 0; i < heldItems.Count; i++)
            {
                if (heldItems[i] == produce)
                {
                    produce.StopProduce();
                    RemoveItem(i);
                    break;
                }
            }
        }

        return isSuccess;
    }

    public void AddAttackingItem(ItemBaseResource item) 
    {
        if(item is IAttack && !attackingItems.Contains(item)) 
        {
            attackingItems.Add(item);
        }
    }

    public void RemoveAttackingItem(ItemBaseResource item)
    {
        if (attackingItems.Contains(item))
        {
            attackingItems.Remove(item);
        }
    }

    private void ProcessAttackingItems(double deltaTime) 
    {
        double addTime = deltaTime * GameManager.instance.gameSpeed;
        if (addTime == 0)
        {
            return;
        }
        List<ItemBaseResource> needRemoves = new List<ItemBaseResource>();

        foreach (var attackingItem in attackingItems)
        {
            if (attackingItem is IAttack attacker)
            {
                if (attacker.nowTime + addTime >= attacker.needTime)
                {
                    bool isSuccess = AttackSomething(attacker);

                    if (isSuccess)
                    {
                        attacker.nowTime = (attacker.nowTime + addTime) % attacker.needTime;

                    }
                    else //找不到可攻擊目標
                    {
                        attacker.nowTime = attacker.needTime;
                    }

                    if(attacker.durability == 0) 
                    {
                        for(int i = 0; i < heldItems.Count; i++) 
                        {
                            if (heldItems[i] == attacker) 
                            {
                                //attacker.StopUsing();
                                //RemoveItem(i);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    attacker.nowTime = (attacker.nowTime + addTime) % attacker.needTime;
                }
            }
            else
            {
                Debug.PrintErr($"不是IAttack, attackingItems index:{attackingItem.index}");
                needRemoves.Add(attackingItem);
            }
        }

        for (int i = 0; i < needRemoves.Count; i++)
        {
            attackingItems.Remove(needRemoves[i]);
        }
    }

    public bool AttackSomething(IAttack attacker)
    {
        bool isAttack = false;

        List<KeyValuePair<double, MonsterObject>> monsters = GameManager.instance.mapManager.FindMonsterInRange(attacker.range);

        if(monsters.Count > 0) 
        {
            for(int i = 0; i < monsters.Count; i++) 
            {
                if (monsters[0].Value.data.nowHp > 0) 
                {
                    if (attacker is ItemBaseResource item) 
                    {
                        MapAttackObject attackObj = GameManager.instance.mapManager.nowMap.CreateMapAttack(item.index, monsters[0].Value.GlobalPosition);
                        attackObj.SetData(item);
                        if (attackObj != null) 
                        {
                            if (attacker.durability > 0) 
                            {
                                attacker.durability = Math.Max(0, attacker.durability - 1);
                                isAttack = true;
                            }
                            else if(attacker.durability == 0) 
                            {
                                isAttack = true;
                            }
                            else if (attacker.durability == -1) 
                            {
                                isAttack = true;
                            }
                        }
                        break;
                    }
                }
            }
        }
        return isAttack;
    }

    public void AddRepairingItem(ItemBaseResource item)
    {
        if (item is IRepair && !repairingItems.Contains(item))
        {
            repairingItems.Add(item);
        }
    }

    public void RemoveRepairingItem(ItemBaseResource item)
    {
        if (repairingItems.Contains(item))
        {
            repairingItems.Remove(item);
        }
    }

    private void ProcessRepairingItems(double deltaTime)
    {
        double addTime = deltaTime * GameManager.instance.gameSpeed;
        if (addTime == 0)
        {
            return;
        }
        List<ItemBaseResource> needRemoves = new List<ItemBaseResource>();

        foreach (var repairingItem in repairingItems)
        {
            if (repairingItem is IRepair repairing)
            {
                if (repairing.nowTime + addTime >= repairing.needTime)
                {
                    bool isSuccess = Repair(repairing);

                    if (isSuccess)
                    {
                        repairing.nowTime = (repairing.nowTime + addTime) % repairing.needTime;

                    }
                    else //不須修復
                    {
                        repairing.nowTime = repairing.needTime;
                    }

                    if (repairing.durability == 0)
                    {
                        for (int i = 0; i < heldItems.Count; i++)
                        {
                            if (heldItems[i] == repairing)
                            {
                                repairing.StopUsing();
                                RemoveItem(i);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    repairing.nowTime = (repairing.nowTime + addTime) % repairing.needTime;
                }
            }
            else
            {
                Debug.PrintErr($"不是IRepair, repairingItems index:{repairingItem.index}");
                needRemoves.Add(repairingItem);
            }
        }

        for (int i = 0; i < needRemoves.Count; i++)
        {
            repairingItems.Remove(needRemoves[i]);
        }
    }

    private bool Repair(IRepair repairing) 
    {
        bool isSuccess = false;
        if (GameManager.instance.battleManager.nowHP < GameManager.instance.battleManager.maxHP)
        {
            if(repairing.durability > 0) 
            {
                repairing.durability = Math.Max(0, repairing.durability - 1);
                GameManager.instance.battleManager.Repair(repairing.repairPoint);
                isSuccess = true;
            }
            else if (repairing.durability == -1) 
            {
                GameManager.instance.battleManager.Repair(repairing.repairPoint);
                isSuccess = true;
            }
        }

        return isSuccess;
    }

    public void AddCoreingItem(ItemBaseResource item) 
    {
        if (item is ICore && !coreingItems.Contains(item))
        {
            coreingItems.Add(item);
        }
    }
    public void RemoveCoreingItem(ItemBaseResource item)
    {
        if (coreingItems.Contains(item))
        {
            coreingItems.Remove(item);
        }
    }

    private void ProcessCoreingItems(double deltaTime) 
    {
        double addTime = deltaTime * GameManager.instance.gameSpeed;
        if (addTime == 0)
        {
            return;
        }
        List<ItemBaseResource> needRemoves = new List<ItemBaseResource>();

        double maxSpeed = 0;
        double tempAcceleration = 0;
        bool isFindMaxSpeed = false;
        foreach (var coreingItem in coreingItems)
        {
            if (coreingItem is ICore core)
            {
                tempAcceleration += core.acceleration;

                if (!isFindMaxSpeed || core.maxSpeed < maxSpeed) 
                {
                    maxSpeed = core.maxSpeed;
                    isFindMaxSpeed = true;
                }
            }
            else
            {
                Debug.PrintErr($"不是ICore, coreItems index:{coreingItem.index}");
                needRemoves.Add(coreingItem);
            }
        }

        if(tempAcceleration != 0) 
        {
            moveAcceleration = tempAcceleration;
        }
        else 
        {
            moveAcceleration = brakeAcceleration;
        }

        if (isTouch) 
        {
            moveSpeed = Math.Clamp(addTime * crashAcceleration + moveSpeed, 0, 20);
        }
        else if (moveSpeed < maxSpeed) 
        {
            moveSpeed = Math.Clamp(addTime * moveAcceleration + moveSpeed, 0, maxSpeed);
        }
        else 
        {
            moveSpeed = Math.Max(addTime * brakeAcceleration + moveSpeed, maxSpeed);
        }

        //Debug.Print($"moveAcceleration:{moveAcceleration}, maxSpeed:{maxSpeed}, moveSpeed:{moveSpeed}");


        for (int i = 0; i < needRemoves.Count; i++)
        {
            coreingItems.Remove(needRemoves[i]);
        }
    }

    public List<ItemBaseResource> GetPickItems(int itemNum) 
    {
        List<ItemBaseResource> result = new List<ItemBaseResource>();

        List<ItemBaseResource> tempItems = new List<ItemBaseResource>();
        foreach(var KV in GameManager.instance.itemConfig.config) 
        {
            if(KV.Value is ToolResource tool) 
            {
                tempItems.Add(KV.Value);
            }
            else if(KV.Value is RecipeResource recipe) 
            {
                bool isFail = false;
                for(int i = 0; i < recipe.materials.Count; i++) 
                {
                    bool isFind = false;
                    for(int j = 0; j < heldItems.Count; j++) 
                    {
                        if (heldItems[j]?.index == recipe.materials[i] || (heldItems[j] is ToolResource heldTool && heldTool.productItem == recipe.materials[i])) 
                        {
                            isFind = true;
                            break;
                        }
                    }

                    if (!isFind) 
                    {
                        isFail = true;
                        break;
                    }
                }

                if (!isFail) 
                {
                    tempItems.Add(KV.Value);
                }
            }
        }


        if(tempItems.Count <= itemNum) 
        {
            result = tempItems;
        }
        else 
        {
            List<int> rndList = GameManager.instance.randomManager.GetNotRepeatList(RandomType.PickItem, 0, tempItems.Count, itemNum);

            for(int i = 0; i < rndList.Count; i++) 
            {
                result.Add(tempItems[rndList[i]]);
            }
        }

        return result;
    }

    public int GetBuyMoney(ItemBaseResource item) 
    {
        int result = 0;
        result = item.money * 2;
        return result;
    }

    public int GetSellMoney(ItemBaseResource item) 
    {
        int result = item.money;

        if(item is IAttack attacker) 
        {
            if(attacker.durability == 0) //磨損
            {
                result = (int)Math.Floor(item.money / 2.0);
            }
        }

        return result;
    }

    public void RefreshAreas(Godot.Collections.Array<AreaIndex> targetArea, Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> useUtemEffects) 
    {
        //Debug.Print("RefreshAreas");
        ClearArea(targetArea);
        for (int i = 0; i < itemNum; i++)
        {
            if(useUtemEffects.TryGetValue(i, out Godot.Collections.Array<ItemEffect> records)) 
            {
                for(int j = 0; j < records.Count; j++) 
                {
                    AddAreaEffect(targetArea, i, records[j].type);
                }
            }
        }
    }

    public int GetEffectPosition(int itemPosition, Vector2I effectPosition)
    {
        int result = -1;
        Vector2I itemVector2 = new Vector2I(itemPosition % itemColumnNum, itemPosition / itemColumnNum);
        
        int x = itemVector2.X + effectPosition.X;
        int y = itemVector2.Y + effectPosition.Y;
        if ((x < itemColumnNum && x >= 0) && 
            (y < itemColumnNum && y >= 0)) 
        {
            result = y * itemColumnNum + x;
            if(result >= itemNum) 
            {
                result = -1;
            }
        }

        return result;
    }

    private void ClearArea(Godot.Collections.Array<AreaIndex> target) 
    {
        target.Clear();
        for (int i = 0; i < itemNum; i++) 
        {
            if (i < defaultAreas.Count && defaultAreas != null) 
            {
                target.Add(defaultAreas[i]);
            }
            else 
            {
                target.Add(AreaIndex.None);
            }
        }
    }

    private void AddAreaEffect(Godot.Collections.Array<AreaIndex> targetArea, int position, AreaIndex effect) 
    {
        targetArea[position] = targetArea[position] | effect;
    }

    private void RemoveAreaEffect(Godot.Collections.Array<AreaIndex> targetArea, int position, AreaIndex effect) 
    {
        targetArea[position] = ~(targetArea[position] | effect);
    }

    public bool IsCanRemove(ItemBaseResource item)
    {
        bool result = false;
        int index = -1;

        for (int i = 0; i < heldItems.Count; i++)
        {
            if (item != null && heldItems[i] == item)
            {
                index = i;
                break;
            }
        }

        Godot.Collections.Array<AreaIndex> tempArea = new Godot.Collections.Array<AreaIndex>();
        Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> tempItemEffects = new Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>>();
        foreach (var KV in itemEffects)
        {
            tempItemEffects.Add(KV.Key, new Godot.Collections.Array<ItemEffect>(KV.Value));
        }

        if (index != -1)
        {
            RemoveItemEffects(tempItemEffects, index, item.effectRanges);
        }

        RefreshAreas(tempArea, tempItemEffects);

        bool isFindError = false;
        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] != null && !IsAreaEffect(tempArea, i, AreaIndex.Normal))
            {
                isFindError = true;
                //Debug.Print($"IsCanRemove FindError i:{i}");
                break;
            }
        }

        if (!isFindError)
        {
            result = true;
        }
        //Debug.Print($"IsCanRemove result:{result}");
        return result;
    }

    public bool IsCanPut(int position, ItemBaseResource item = null) 
    {
        bool result = false;
        int index = -1;

        for(int i = 0; i < heldItems.Count; i++) 
        {
            if (item != null && heldItems[i] == item) 
            {
                index = i;
                break;
            }
        }

        ItemBaseResource positionItem = null;
        if (position >= 0 && position < heldItems.Count) 
        {
            positionItem = heldItems[position];
        }
        Godot.Collections.Array<AreaIndex> tempArea = new Godot.Collections.Array<AreaIndex>();
        Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> tempItemEffects = new Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>>();
        foreach(var KV in itemEffects) 
        {
            tempItemEffects.Add(KV.Key, new Godot.Collections.Array<ItemEffect>(KV.Value));
        }

        if (index != -1)
        {
            RemoveItemEffects(tempItemEffects, index, item.effectRanges);
        }
        if(positionItem != null) 
        {
            RemoveItemEffects(tempItemEffects, position, positionItem.effectRanges);
        }

        RefreshAreas(tempArea, tempItemEffects);

        bool isTargetNormal = false;

        //若放置目標地是可放置
        if(IsAreaEffect(tempArea, position, AreaIndex.Normal)) 
        {
            isTargetNormal = true;
        }

        if (!isTargetNormal) 
        {
            //Debug.Print($"IsCanPut isTargetNormal false");
            return result;
        }

        AddItemEffects(tempItemEffects, position, item.effectRanges);
        if(positionItem != null) 
        {
            AddItemEffects(tempItemEffects, index, positionItem.effectRanges);
        }

        RefreshAreas(tempArea, tempItemEffects);
        bool isFindError = false;
        for(int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] != null && !IsAreaEffect(tempArea, i, AreaIndex.Normal)) 
            {
                isFindError = true;
                //Debug.Print($"IsCanPut FindError i:{i}");
                break;
            }
        }

        if (isTargetNormal && !isFindError) 
        {
            result = true;
        }
        //Debug.Print($"IsCanPut result:{result}");
        return result;
    }

    public bool IsAreaEffect(Godot.Collections.Array<AreaIndex> targetArea, int position, AreaIndex effect) 
    {
        AreaIndex testAreaIndex = AreaIndex.None;
        if (position >= 0 && position < targetArea.Count)
        {
            testAreaIndex = targetArea[position];
        }

        return (testAreaIndex & effect) == effect;
    }

    private void AddItemEffects(Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> target, int position, Godot.Collections.Array<ItemEffect> itemEffects) 
    {
        for(int i = 0; i < itemEffects.Count; i++) 
        {
            int effectPosition = GetEffectPosition(position, itemEffects[i].position);
            if (effectPosition >= 0 && effectPosition < itemNum) 
            {
                AddItemEffect(target, effectPosition, itemEffects[i]);
            }
        }
    }

    private void AddItemEffect(Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> target, int position, ItemEffect itemEffect) 
    {
        if (!target.ContainsKey(position)) 
        {
            target.Add(position, new Godot.Collections.Array<ItemEffect>());
        }

        Godot.Collections.Array<ItemEffect> effects = target[position];
        if(FindItemEffect(effects, itemEffect) == -1) 
        {
            effects.Add(itemEffect);
            isAreasDirt = true;
        }
    }

    private void RemoveItemEffects(Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> target, int position, Godot.Collections.Array<ItemEffect> itemEffects)
    {
        for (int i = 0; i < itemEffects.Count; i++)
        {
            int effectPosition = GetEffectPosition(position, itemEffects[i].position);
            if (effectPosition >= 0 && effectPosition < itemNum)
            {
                RemoveItemEffect(target,effectPosition, itemEffects[i]);
            }
        }
    }

    private void RemoveItemEffect(Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> target, int position, ItemEffect itemEffect) 
    {
        if (target.TryGetValue(position, out Godot.Collections.Array<ItemEffect> recordEffects))
        {
            int recordPos = FindItemEffect(recordEffects, itemEffect);
            if (recordPos != -1)
            {
                //Debug.Print($"RemoveItemEffect itemEffect:{itemEffect}");
                recordEffects.RemoveAt(recordPos);
                isAreasDirt = true;
                if(recordEffects.Count == 0) 
                {
                    target.Remove(position);
                }
            }
            else 
            {
                Debug.PrintWarn($"RemoveItemEffect itemEffect 紀錄不存在 itemEffect:{itemEffect}");
            }
        }
        else 
        {
            Debug.PrintWarn($"RemoveItemEffect position 紀錄不存在 position:{position}");
        }
    }

    private int FindItemEffect(Godot.Collections.Array<ItemEffect> effects, ItemEffect effect) 
    {
        int result = -1;
        for(int i = 0; i < effects.Count; i++) 
        {
            if (effects[i].IsSame(effect)) 
            {
                result = i;
                break;
            }
        }

        return result;
    }

    private void GetItemEffectRangePositions(int index, ref HashSet<int> result) 
    {
        for(int i = 0; i < heldItems[index].effectRanges.Count; i++) 
        {
            int effectPosition = GetEffectPosition(index, heldItems[index].effectRanges[i].position);
            if (effectPosition >= 0 && effectPosition < itemNum)
            {
                if (!result.Contains(effectPosition)) 
                {
                    result.Add(effectPosition);
                }
            }
        }
    }

    //依據新增1個道具(並且可減少1個道具)的變化得出變化後的Areas
    public Godot.Collections.Array<AreaIndex> TempAreaChange(ItemBaseResource item, int itemPosition, int removeItemPosition = -1) 
    {
        Godot.Collections.Array<AreaIndex> tempArea = new Godot.Collections.Array<AreaIndex>();
        Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>> tempItemEffects = new Godot.Collections.Dictionary<int, Godot.Collections.Array<ItemEffect>>();
        foreach (var KV in itemEffects)
        {
            tempItemEffects.Add(KV.Key, new Godot.Collections.Array<ItemEffect>(KV.Value));
        }

        if(removeItemPosition != -1) 
        {
            RemoveItemEffects(tempItemEffects, removeItemPosition, heldItems[removeItemPosition].effectRanges);
        }
        AddItemEffects(tempItemEffects, itemPosition, item.effectRanges);
        RefreshAreas(tempArea, tempItemEffects);

        return tempArea;
    }

    //從參考點依順序取得周圍的位置, 順序大致如下
    // 7,8,9
    // 6,1,2
    // 5,4,3
    public List<int> GetPointAroundPoints(int position) 
    {
        List<int> result = new List<int>();

        int maxDir = 0;
        int addX = 0;
        int addY = 0;

        int type = 0;

        Vector2I vectorPos = new Vector2I(position % itemColumnNum, position / itemColumnNum);
        Action checkAndAddResult = () =>
        {
            Vector2I tempVectorPos = new Vector2I(vectorPos.X + addX, vectorPos.Y + addY);
            if ((tempVectorPos.X < itemColumnNum && tempVectorPos.X >= 0) &&
                (tempVectorPos.Y < itemLineNum && tempVectorPos.Y >= 0))
            {
                result.Add(tempVectorPos.Y * itemColumnNum + tempVectorPos.X);
            }
        };

        while (result.Count < itemNum) 
        {
            if(maxDir == 0) 
            {
                result.Add(position);
                maxDir += 1;
            }
            else 
            {
                switch (type) 
                {
                    case 0:
                        {
                            addX = maxDir;
                            checkAndAddResult();

                            if (Math.Abs(addY + 1) <= maxDir) 
                            {
                                addY++;
                            }
                            else
                            {
                                type = 1;
                                addX--;
                            }
                        }
                        break;
                    case 1:
                        {
                            checkAndAddResult();

                            if (Math.Abs(addX - 1) <= maxDir) 
                            {
                                addX--;
                            }
                            else 
                            {
                                type = 2;
                                addY--;
                            }
                        }
                        break;
                    case 2: 
                        {
                            checkAndAddResult();

                            if (Math.Abs(addY - 1) <= maxDir)
                            {
                                addY--;
                            }
                            else
                            {
                                type = 3;
                                addX++;
                            }
                        }
                        break;
                    case 3:
                        {
                            checkAndAddResult();

                            if (Math.Abs(addX + 1) <= maxDir)
                            {
                                addX++;
                            }
                            else
                            {
                                type = 4;
                                addY++;
                            }
                        }
                        break;
                    case 4: 
                        {
                            checkAndAddResult();

                            if (addX == maxDir && addY == 0) 
                            {
                                type = 0;
                                maxDir++;
                            }
                            else 
                            {
                                addY++;
                            }
                        }
                        break;
                }
            }
        }

        //Debug.Print($"GetPointAroundPoints position:{position}, result:{result.ToStringExtended()}");

        return result;
    }

    public void AddFeature(FeatureIndex featureIndex) 
    {
        if (!haveFeatures.Contains(featureIndex)) 
        {
            haveFeatures.Add(featureIndex);
        }
        else 
        {
            Debug.PrintErr($"已擁有該能力, featureIndex:{featureIndex}");
        }
    }
}
