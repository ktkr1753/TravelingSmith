using Godot;
using System;
using System.Collections.Generic;

public partial class ShopUI : UIBase
{
	[Export] private ShopItemElementPool itemPool;

	public List<ItemBaseResource> sellItems = new List<ItemBaseResource>();


    public override void Init()
    {
        base.Init();

        //測試
        TestRandomItem();

        SetView();
    }

    private void TestRandomItem() 
    {
        List<ItemIndex> indexs = new List<ItemIndex>();
        foreach (ItemIndex itemIndex in GameManager.instance.itemConfig.config.Keys) 
        {
            indexs.Add(itemIndex);
        }

        for(int i = 0; i < 5; i++) 
        {
            int rnd = GameManager.instance.randomManager.GetRange(RandomType.Other, 0, indexs.Count);

            ItemBaseResource item = GameManager.instance.itemConfig.config[indexs[rnd]];
            sellItems.Add(item);
        }
    }

    private void SetView() 
    {
        SetShopItemElements();
    }

    private void SetShopItemElements() 
    {
        ClearShopItemElements();
        for (int i = 0; i < sellItems.Count; i++) 
        {
            ShopItemElement element = itemPool.GetElement();
            element.SetData(i, sellItems[i]);
        }
    }

    private void ClearShopItemElements() 
    {
        for(int i = 0; i < itemPool.inuses.Count; i++) 
        {
            itemPool.inuses[i].SetData(-1, null);
        }
        itemPool.ReturnAllElement();
    }
}
