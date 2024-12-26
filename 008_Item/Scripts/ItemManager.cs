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

    private Godot.Collections.Array<ItemBaseResource> _heldItems = new Godot.Collections.Array<ItemBaseResource>();
    [Export] public Godot.Collections.Array<ItemBaseResource> heldItems 
	{
		get { return _heldItems; }
		private set { _heldItems = value; }
	}

    public ItemBaseResource waitAddItem = null;

    private Godot.Collections.Array<AreaResource> _areas = new Godot.Collections.Array<AreaResource>();

    [Export] public Godot.Collections.Array<AreaResource> areas 
    {
        get { return _areas; }
        private set { _areas = value; }
    }

    public bool isHeldItemFull 
    {
        get 
        {
            bool isAnyEmpty = false;

            for(int i = 0; i < _heldItems.Count; i++) 
            {
                if (_heldItems[i] == null) 
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
    public event Action<IMake, HashSet<KeyValuePair<int, ItemBaseResource>>> onUseMaterial;         //<make, <usedItemIndexs, usedItem>>
    public event Action<IProduce, int, ItemBaseResource> onCreateProduct;       //<produce, productItemIndex, productItem>

    public void Init() 
	{
		//測試用道具
        for (int i = 0; i < 25; i++)
        {
            ItemBaseResource item = null;

            if (i == 0)
            {
                item = GameManager.instance.itemManager.CreateItem(ItemIndex.Pickaxe);
            }
            else if(i == 1) 
            {
                item = GameManager.instance.itemManager.CreateItem(ItemIndex.RecipeDart);
            }
            else if (i == 2)
            {
                item = GameManager.instance.itemManager.CreateItem(ItemIndex.WoodenWheel);
            }
            else if (i == 3)
            {
                item = GameManager.instance.itemManager.CreateItem(ItemIndex.IronWheel);
            }

            heldItems.Add(item);
        }
    }

    public int AddHeldItem(ItemIndex itemIndex) 
    {
        int index = -1;
        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] == null)
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
            SetHeldItem(index, null);
        }
    }

	public void SetHeldItem(int index, ItemBaseResource item) 
	{
        if (index >= 0 && index < heldItems.Count) 
		{
            heldItems[index] = item;
			onHeldItemChange?.Invoke(index);
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
            money += heldItems[index].money;
            GameManager.instance.itemManager.RemoveItem(index);
            isSuccess = true;
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
        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] == null)
            {
                ItemBaseResource item = CreateItem(produce.productItem);
                SetHeldItem(i, item);
                isSuccess = true;
                onCreateProduct?.Invoke(produce, i, item);
                break;
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

                    if(attacker.durability <= 0) 
                    {
                        for(int i = 0; i < heldItems.Count; i++) 
                        {
                            if (heldItems[i] == attacker) 
                            {
                                attacker.StopUsing();
                                RemoveItem(i);
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

    public bool AttackSomething(IAttack attcker)
    {
        bool isAttack = false;

        List<KeyValuePair<double, MonsterObject>> monsters = GameManager.instance.mapManager.FindMonsterInRange(attcker.range);

        if(attcker.durability > 0 && monsters.Count > 0) 
        {
            for(int i = 0; i < monsters.Count; i++) 
            {
                if (monsters[0].Value.data.nowHp > 0) 
                {
                    monsters[0].Value.data.Damage(attcker.attackPoint);
                    attcker.durability = Math.Max(0, attcker.durability - 1);
                    GameManager.instance.mapManager.PlayFX(attcker.fx, monsters[0].Value.GlobalPosition);
                    isAttack = true;
                    break;
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

                    if (repairing.durability <= 0)
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
        if(repairing.durability > 0 && GameManager.instance.battleManager.nowHP < GameManager.instance.battleManager.maxHP) 
        {
            repairing.durability = Math.Max(0, repairing.durability - 1);
            GameManager.instance.battleManager.Repair(repairing.repairPoint);
            isSuccess = true;
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


        if (moveSpeed < maxSpeed) 
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

        if(item is IProduce) 
        {
            result = item.money * 5;
        }
        else 
        {
            result = item.money * 2;
        }

        return result;
    }

}
