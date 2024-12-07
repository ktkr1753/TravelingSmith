using Godot;
using System;
using System.Collections.Generic;

public partial class ItemManager : Node
{
    public int money = 0;

	private Godot.Collections.Array<ItemBaseResource> _heldItems = new Godot.Collections.Array<ItemBaseResource>();
    [Export] public Godot.Collections.Array<ItemBaseResource> heldItems 
	{
		get { return _heldItems; }
		private set { _heldItems = value; }
	}

    public double gameSpeed
    {
        get
        {
            double result = 1;
            if (GameManager.instance.battleManager.isGameOver)
            {
                result = 0;
            }
            else
            {
                result = GameManager.instance.localSetting.gameSpeedSetting;
            }
            return result;
        }
    }

    HashSet<ItemBaseResource> producingItems = new HashSet<ItemBaseResource>();

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
            else if (i == 24)
            {
                item = GameManager.instance.itemManager.CreateItem(ItemIndex.RecipeDart);
            }

            heldItems.Add(item);
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

    public bool TryMake(IMake make) 
    {
        return TryMake(make, out HashSet<int> usedItemsIndex);
    }

	public bool TryMake(IMake make, out HashSet<int> usedItemsIndex) 
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
                else if (make.materials[i] == GameManager.instance.itemManager.heldItems[j]?.index)
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

    public bool Make(IMake make) 
    {
        bool result = false;
        if(TryMake(make, out HashSet<int> usedItemsIndex)) 
        {
            HashSet<KeyValuePair<int, ItemBaseResource>> items = new HashSet<KeyValuePair<int, ItemBaseResource>>();
            foreach (int index in usedItemsIndex)
            {
                items.Add(new KeyValuePair<int, ItemBaseResource>(index, heldItems[index]));
                SetHeldItem(index, null);
            }

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
        double addTime = deltaTime * gameSpeed;

        List<ItemBaseResource> needRemoves = new List<ItemBaseResource>();

		foreach(var producingItem in producingItems) 
		{
			if (producingItem is IProduce produce)
			{
				if(produce.nowTime + addTime > produce.needTime) 
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
                    }
                    else //物品太多生產失敗，不做事 
                    {

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
        bool isCreate = false;
        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] == null)
            {
                ItemBaseResource item = CreateItem(produce.productItem);
                SetHeldItem(i, item);
                isCreate = true;
                onCreateProduct?.Invoke(produce, i, item);
                break;
            }
        }

        return isCreate;
    }
}
