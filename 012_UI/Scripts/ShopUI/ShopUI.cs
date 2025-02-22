using Godot;
using System;
using System.Collections.Generic;

public partial class ShopUI : UIBase
{
    [Export] private Label refreshCostLabel;
    [Export] private Button refreshButton;
    [Export] private ShopItemElementPool itemPool;
    [Export] private TextureRect assignMaterialImage;
    [Export] private Label assignMaterialMoneyLabel;
    [Export] private Control sellPanel;
    [Export] private Control refreshHeldPanel;
    [Export] private Label refreshHeldCostLabel;
    [Export] private Button multiSellButton;
    [Export] private Button getRecipeButton;
    [Export] private Label getRecipeCostLabel;

    private int _nowPickElementIndex = -1;
    public int nowPickElementIndex
    {
        get { return _nowPickElementIndex; }
        set
        {
            if (_nowPickElementIndex != value)
            {
                int preState = _nowPickElementIndex;
                _nowPickElementIndex = value;

                if (preState >= 0 && preState < itemPool.inuses.Count)
                {
                    itemPool.inuses[preState].itemElement.isPicking = false;
                }

                if (_nowPickElementIndex >= 0 && _nowPickElementIndex < itemPool.inuses.Count)
                {
                    SetPickedItemElement(itemPool.inuses[_nowPickElementIndex].item);
                    itemPool.inuses[_nowPickElementIndex].itemElement.isPicking = true;
                }
                else
                {
                    SetPickedItemElement(null);
                }
            }
        }
    }

    private int refreshCount = 0;
    public int refreshCost 
    {
        get 
        {
            int result = 0;
            result = (int)Math.Ceiling(GameManager.instance.mapManager.nowMap.visitedShopIndex * 0.3) + 1 + (int)Math.Ceiling(refreshCount * 1.5);

            return result;
        }
    }

    private static int refreshHeldCount = 0;

    public int refreshHeldCost 
    {
        get
        {
            int result = 0;
            result = 1 + (int)(Math.Ceiling(refreshHeldCount * 0.6));
            return result;
        }
    }

    public List<ItemBaseResource> sellItems = new List<ItemBaseResource>();

    public List<ItemBaseResource> assignMaterialItems = new List<ItemBaseResource>();
    private int _nowAssignMaterialIndex = -1;
    public int nowAssignMaterialIndex 
    {
        get 
        {
            return _nowAssignMaterialIndex;
        }
        set 
        {
            if (_nowAssignMaterialIndex != value) 
            {
                int preState = _nowAssignMaterialIndex;
                _nowAssignMaterialIndex = value;
                OnAssignMaterialChange(preState, _nowAssignMaterialIndex);
            }
        }
    }

    private ItemBaseResource pickAssignMaterialItem = null;

    private void OnAssignMaterialChange(int preIndex, int nextIndex) 
    {
        if(nextIndex >= 0 && nextIndex < assignMaterialItems.Count) 
        {
            SetAssignMaterialImage();
            SetAssignMaterialMoney();
        }
    }

    private bool _isMultiSelectedSell = false;
    public bool isMultiSelectedSell 
    {
        get { return _isMultiSelectedSell; }
        set 
        {
            if(_isMultiSelectedSell != value ) 
            {
                _isMultiSelectedSell = value;
                if (_isMultiSelectedSell) 
                {
                    RegistryItemElement();
                }
                else
                {
                    SellAllMultiSelectedItem();
                    UnregistryItemElement();
                }
                SetMultiButton();
            }
        }
    }


    public HashSet<int> selectedItemIndexs = new HashSet<int>();

    private Action pauseFinishCallback;

    public const double flyTime = 0.2;
    public const int sellRecipeNum = 0;

    public const string clip_moveIn = "moveIn";
    public const string clip_moveOut = "moveOut";

    public override void Init()
    {
        base.Init();

        if (GameManager.instance.itemManager.featureContents.Contains(FeatureContentIndex.LoseMoney)) 
        {
            GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - 2);
        }

        GameManager.instance.itemManager.onMoneyChange += OnMoneyChange;
        pauseFinishCallback = GameManager.instance.AddNeedPause();

        //測試
        TestRandomItem();

        SetView();
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        pauseFinishCallback?.Invoke();
        pauseFinishCallback = null;

        GameManager.instance.itemManager.onMoneyChange -= OnMoneyChange;
        UnregistryItemElement();
    }

    private void TestRandomItem() 
    {
        sellItems.Clear();
        List<ItemIndex> materialIndexs = new List<ItemIndex>();
        List<ItemIndex> recipeIndexs = new List<ItemIndex>();
        HashSet<ItemIndex> waitUnlockRecipe = GameManager.instance.unlockRecipe.GetWaitUnlockRecipe();
        //Debug.Print($"waitUnlockRecipe:{waitUnlockRecipe.ToStringExtended()}");
        foreach (ItemIndex itemIndex in GameManager.instance.itemConfig.config.Keys) 
        {
            if (GameManager.instance.itemConfig.config.TryGetValue(itemIndex, out ItemBaseResource item)) 
            {
                if(item is MaterialResource && item.isSellable) 
                {
                    switch (itemIndex) 
                    {
                        case ItemIndex.Iron:
                            {
                                if (GameManager.instance.unlockRecipe.unlockedRecipes.Contains(ItemIndex.RecipeIronForge)) 
                                {
                                    materialIndexs.Add(itemIndex);
                                }
                            }
                            continue;
                        case ItemIndex.Titanium:
                            {
                                if (GameManager.instance.unlockRecipe.unlockedRecipes.Contains(ItemIndex.RecipeTitaniumForge))
                                {
                                    materialIndexs.Add(itemIndex);
                                }
                            }
                            continue;
                        case ItemIndex.FlareGemstoneOre:
                            {
                                if (GameManager.instance.unlockRecipe.unlockedRecipes.Contains(ItemIndex.RecipeFlareGemstonePickaxe))
                                {
                                    materialIndexs.Add(itemIndex);
                                }
                            }
                            continue;
                        case ItemIndex.FlareGemstone: 
                            {
                                if (GameManager.instance.unlockRecipe.unlockedRecipes.Contains(ItemIndex.RecipeFlareGemstoneBurin))
                                {
                                    materialIndexs.Add(itemIndex);
                                }
                            }
                            break;
                        case ItemIndex.IronOre:  
                            {
                                if (GameManager.instance.unlockRecipe.unlockedRecipes.Contains(ItemIndex.RecipePickaxe))
                                {
                                    materialIndexs.Add(itemIndex);
                                }
                            }
                            continue;
                        case ItemIndex.GoldOre:
                            {
                                materialIndexs.Add(itemIndex);
                            }
                            continue;
                    }
                }
                else if(item is RecipeResource recipe && item.detailType == ItemDetailType.Paper && recipe.isSellable) 
                {
                    if (GameManager.instance.unlockRecipe.unlockedRecipes.Contains(recipe.index) || waitUnlockRecipe.Contains(recipe.index)) 
                    {
                        recipeIndexs.Add(itemIndex);
                    }
                }
                else if(item is SelfToolResource selfTool && selfTool.isSellable) 
                {
                    materialIndexs.Add(itemIndex);
                }
            }
        }

        //Debug.Print($"materialIndexs:{materialIndexs.ToStringExtended()}");
        //Debug.Print($"recipeIndexs:{recipeIndexs.ToStringExtended()}");

        for (int i = 0; i < sellRecipeNum; i++) 
        {
            ItemIndex itemIndex = ItemIndex.None;
            if (i < sellRecipeNum)
            {
                if(recipeIndexs.Count == 0) 
                {

                }
                else 
                {
                    int rnd = GameManager.instance.randomManager.GetRange(RandomType.Other, 0, recipeIndexs.Count);
                    itemIndex = recipeIndexs[rnd];
                }
            }
            else 
            {
                int rnd = GameManager.instance.randomManager.GetRange(RandomType.Other, 0, materialIndexs.Count);
                itemIndex = materialIndexs[rnd];
            }

            ItemBaseResource item = GameManager.instance.itemConfig.config[itemIndex];
            sellItems.Add(item);
        }
    }

    private void RefrershItem() 
    {
        GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - refreshCost);
        TestRandomItem();
        SetShopItemElements();

        refreshCount++;
        SetRefreshButton();
    }

    private void RegistryItemElement() 
    {
        MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
        if(mainGameUI == null) 
        {
            Debug.PrintWarn("未找到ItemElement");
            return;
        }

        for(int i = 0; i < mainGameUI.elements.Count; i++) 
        {
            mainGameUI.elements[i].onMainButtonDown += OnElementButtonDown;
        }
    }

    private void UnregistryItemElement()
    {
        MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
        if (mainGameUI == null)
        {
            Debug.PrintWarn("未找到ItemElement");
            return;
        }

        for (int i = 0; i < mainGameUI.elements.Count; i++)
        {
            mainGameUI.elements[i].SetMultiSelected(false);
            mainGameUI.elements[i].onMainButtonDown -= OnElementButtonDown;
        }
    }


    private void SetView() 
    {
        SetShopItemElements();
        ResetAssignMaterial();
        SetRefreshButton();
        SetRefreshHeldCost();
        SetGetRecipeButton();
    }
    
    private void SetShopItemElements() 
    {
        ClearShopItemElements();
        for (int i = 0; i < sellItems.Count; i++) 
        {
            ShopItemElement element = itemPool.GetElement();
            element.SetData(i, sellItems[i]);
            element.itemElement.onMainButtonDown += OnShopItemButtonDown;
            element.itemElement.onMainButtonUp += OnShopItemButtonUp;
        }
    }

    private void ClearShopItemElements() 
    {
        for(int i = 0; i < itemPool.inuses.Count; i++) 
        {
            ClearShopItemElementData(itemPool.inuses[i]);
        }
        itemPool.ReturnAllElement();
    }

    private void ClearShopItemElementData(ShopItemElement element) 
    {
        element.SetData(-1, null);
        element.itemElement.onMainButtonDown -= OnShopItemButtonDown;
        element.itemElement.onMainButtonUp -= OnShopItemButtonUp;
    }

    private void ResetAssignMaterial()
    {
        assignMaterialItems.Clear();
        foreach (var KV in GameManager.instance.itemConfig.config)
        {
            if ((int)KV.Value.index > 600 && (int)KV.Value.index <= 700) //是素材
            {
                assignMaterialItems.Add(KV.Value);
            }
        }

        assignMaterialItems.Sort((a, b) =>
        {
            return (int)(a.index - b.index);
        });

        nowAssignMaterialIndex = 0;
    }

    private void SetAssignMaterialImage()
    {
        ItemBaseResource item = assignMaterialItems[nowAssignMaterialIndex];
        assignMaterialImage.Texture = item.texture;
    }

    private void SetAssignMaterialMoney()
    {
        ItemBaseResource item = assignMaterialItems[nowAssignMaterialIndex];
        int buyMoney = GameManager.instance.itemManager.GetBuyMoney(item);
        if (GameManager.instance.itemManager.money >= buyMoney)
        {
            assignMaterialMoneyLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
        }
        else
        {
            assignMaterialMoneyLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
        }
        assignMaterialMoneyLabel.Text = $"{buyMoney}";
    }

    public void SellAllMultiSelectedItem() 
    {
        bool isSell = false;
        foreach(int index in selectedItemIndexs) 
        {
            ItemBaseResource item = GameManager.instance.itemManager.heldItems[index];
            if (item != null && item.isSellable && GameManager.instance.itemManager.IsCanRemove(item)) 
            {
                bool isSuccess = GameManager.instance.itemManager.SellItem(item);

                if (isSuccess)
                {
                    isSell = true;
                }
            }
        }

        selectedItemIndexs.Clear();

        if (isSell) 
        {
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_get_money);
        }
    }

    public bool IsInSellRect(Vector2 globalPosition) 
    {
        bool result = false;
        Rect2 sellRect = new Rect2(sellPanel.GlobalPosition, sellPanel.Size);
        if (sellRect.HasPoint(globalPosition)) 
        {
            result = true;
        }

        return  result;
    }

    public bool IsInRefreshRect(Vector2 globalPosition) 
    {
        bool result = false;
        Rect2 refreshRect = new Rect2(refreshHeldPanel.GlobalPosition, sellPanel.Size);
        if (refreshRect.HasPoint(globalPosition))
        {
            result = true;
        }

        return result;
    }

    public void SetItemInfo(ItemBaseResource item, bool interactable = true) 
    {
        MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
        if (mainGameUI != null) 
        {
            mainGameUI.SetItemInfoPanel(item, interactable);
        }
    }

    private void OnShopItemButtonDown(int index) 
    {
        //Debug.Print($"OnShopItemButtonDown index:{index}");
        int inuseOrder = -1;
        for(int i = 0; i < itemPool.inuses.Count; i++) 
        {
            if (itemPool.inuses[i].index == index) 
            {
                inuseOrder = i;
                break;
            }
        }

        if (inuseOrder != -1) 
        {
            ShopItemElement element = itemPool.inuses[inuseOrder];

            if(GameManager.instance.itemManager.money >= element.money) 
            {
                nowPickElementIndex = inuseOrder;
            }

            SetItemInfo(element.item.Clone(), false);
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_1);
        }
    }

    private void OnShopItemButtonUp(int index) 
    {
        bool isCostMoney = false;
        ItemBaseResource pickedItem = null;
        ShopItemElement element = null;
        if (nowPickElementIndex != -1)
        {
            element = itemPool.inuses[nowPickElementIndex];
            pickedItem = element.item;
        }

        if (pickedItem != null) 
        {
            MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
            if (mainGameUI != null)
            {
                if (mainGameUI.nowEnterElementIndex >= 0)
                {
                    int addIndex = -1;
                    if (GameManager.instance.itemManager.heldItems[mainGameUI.nowEnterElementIndex] == null && GameManager.instance.itemManager.IsAreaEffect(GameManager.instance.itemManager.areas, mainGameUI.nowEnterElementIndex, AreaIndex.Normal)) 
                    {
                        addIndex = mainGameUI.nowEnterElementIndex;
                        ItemBaseResource item = GameManager.instance.itemManager.CreateItem(pickedItem.index);
                        GameManager.instance.itemManager.SetHeldItem(addIndex, item);
                    }
                    else 
                    {
                        addIndex = GameManager.instance.itemManager.AddHeldItem(pickedItem.index);
                    }

                    if (addIndex != -1)
                    {
                        //資料
                        sellItems.Remove(pickedItem);
                        GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - element.money);
                        isCostMoney = true;
                        //演出
                        /*
                        mainGameUI.elements[addIndex].isFlying = true;
                        Vector2 endPosition = mainGameUI.elements[addIndex].GlobalPosition;
                        GameManager.instance.uiManager.StartFlyItem(element.item, element.GlobalPosition, endPosition, flyTime, () =>
                        {
                            mainGameUI.elements[addIndex].isFlying = false;
                        });
                        */
                        ClearShopItemElementData(element);
                        itemPool.ReturnElement(element);

                        if(pickedItem is RecipeResource recipe && pickedItem.detailType == ItemDetailType.Paper) 
                        {
                            GameManager.instance.unlockRecipe.AddUnlockRecipe(recipe.index);
                        }

                        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_get_money);
                    }
                }
            }
        }
        nowPickElementIndex = -1;

        if (!isCostMoney) 
        {
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_2);
        }
    }

    private void SetPickedItemElement(ItemBaseResource item)
    {
        if (item != null)
        {
            PickUpUI pickUpUI = GameManager.instance.uiManager.GetOpenedUI<PickUpUI>(UIIndex.PickUpUI);
            if (pickUpUI != null)
            {
                pickUpUI.SetData(item);
            }
            else
            {

                pickUpUI = GameManager.instance.uiManager.OpenUI<PickUpUI>(UIIndex.PickUpUI, new List<object>() { item });
            }
        }
        else
        {
            GameManager.instance.uiManager.CloseUI(UIIndex.PickUpUI);
        }
    }

    private void OnAssignMaterialButtonDown() 
    {
        ItemBaseResource pickedItem = null;
        if (nowAssignMaterialIndex >= 0 && nowAssignMaterialIndex < assignMaterialItems.Count)
        {
            pickedItem = assignMaterialItems[nowAssignMaterialIndex];
        }

        if (pickedItem != null) 
        {
            int buyMoney = GameManager.instance.itemManager.GetBuyMoney(pickedItem);
            if (GameManager.instance.itemManager.money >= buyMoney)
            {
                GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_1);
                SetPickedItemElement(pickedItem);
                assignMaterialImage.Visible = false;
                pickAssignMaterialItem = pickedItem;
            }
            SetItemInfo(pickedItem.Clone(), false);
        }

    }

    private void OnAssignMaterialButtonUp()
    {
        ItemBaseResource pickedItem = pickAssignMaterialItem;

        bool isCostMoney = false;
        if (pickedItem != null)
        {
            MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
            if (mainGameUI != null)
            {
                if (mainGameUI.nowEnterElementIndex >= 0)
                {
                    int addIndex = -1;
                    if (GameManager.instance.itemManager.heldItems[mainGameUI.nowEnterElementIndex] == null && GameManager.instance.itemManager.IsAreaEffect(GameManager.instance.itemManager.areas, mainGameUI.nowEnterElementIndex, AreaIndex.Normal))
                    {
                        addIndex = mainGameUI.nowEnterElementIndex;
                        ItemBaseResource item = GameManager.instance.itemManager.CreateItem(pickedItem.index);
                        GameManager.instance.itemManager.SetHeldItem(addIndex, item);
                    }
                    else
                    {
                        addIndex = GameManager.instance.itemManager.AddHeldItem(pickedItem.index);
                    }

                    if (addIndex != -1)
                    {
                        //資料
                        GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - GameManager.instance.itemManager.GetBuyMoney(pickedItem));
                        isCostMoney = true;

                        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_get_money);
                    }
                }
            }
        }

        if (!isCostMoney) 
        {
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_2);        
        }
        assignMaterialImage.Visible = true;
        SetPickedItemElement(null);
    }

    private void SetRefreshButton() 
    {
        if (GameManager.instance.itemManager.money >= refreshCost)
        {
            refreshButton.Disabled = false;
            refreshCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
        }
        else
        {
            refreshButton.Disabled = true;
            refreshCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
        }
        refreshCostLabel.Text = $"{refreshCost}";
    }

    private void SetRefreshHeldCost() 
    {
        if (GameManager.instance.itemManager.money >= refreshHeldCost)
        {
            refreshHeldCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
        }
        else
        {
            refreshHeldCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
        }
        refreshHeldCostLabel.Text = $"{refreshHeldCost}";
    }

    public bool RefreshHeldItem(int nowPickElementIndex) 
    {
        bool isSuccess = false;

        isSuccess = GameManager.instance.itemManager.RefreshItem(nowPickElementIndex, refreshHeldCost);
        if (isSuccess) 
        {
            refreshHeldCount++;
            SetRefreshHeldCost();
        }
        return isSuccess;
    }


    private void SetMultiButton()
    {
        if (isMultiSelectedSell) 
        {
            if(selectedItemIndexs.Count == 0) 
            {
                multiSellButton.Text = Tr("ts_common_Cancel");
                multiSellButton.ThemeTypeVariation = "Button_Red";
            }
            else 
            {
                multiSellButton.Text = Tr("ts_common_Confirm");
                multiSellButton.ThemeTypeVariation = "Button_Green";
            }
        }
        else 
        {
            multiSellButton.Text = Tr("ts_common_Sell");
            multiSellButton.ThemeTypeVariation = "";
        }
    }

    private void SetGetRecipeButton() 
    {
        if (GameManager.instance.itemManager.money >= refreshHeldCost)
        {
            getRecipeButton.Disabled = false;
            getRecipeCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.normalColor;
        }
        else
        {
            getRecipeButton.Disabled = true;
            getRecipeCostLabel.SelfModulate = GameManager.instance.uiCommonSetting.worseColor;
        }
        getRecipeCostLabel.Text = $"{refreshHeldCost}";
    }

    private void OnMoneyChange(int money) 
    {
        SetRefreshButton();
        SetRefreshHeldCost();
        SetAssignMaterialMoney();
        SetGetRecipeButton();
    }


    public void OnRefreshClick() 
    {
        RefrershItem();
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
    }

    public void OnCloseClick()
    {
        GameManager.instance.uiManager.CloseUI(this);
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_cancel4);
    }

    public void OnLeftMaterialClick() 
    {
        int result = -1;
        if(nowAssignMaterialIndex - 1 >= 0 && nowAssignMaterialIndex -1 < assignMaterialItems.Count) 
        {
            result = nowAssignMaterialIndex - 1;
        }
        else if(assignMaterialItems.Count != 0)
        {
            result = assignMaterialItems.Count - 1;
        }
        nowAssignMaterialIndex = result;
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button32);
    }

    public void OnRightMaterialClick() 
    {
        int result = -1;
        if (nowAssignMaterialIndex + 1 >= 0 && nowAssignMaterialIndex + 1 < assignMaterialItems.Count)
        {
            result = nowAssignMaterialIndex + 1;
        }
        else if (assignMaterialItems.Count != 0)
        {
            result = 0;
        }
        nowAssignMaterialIndex = result;
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button32);
    }

    public void OnMultiSelectedClick() 
    {
        isMultiSelectedSell = !isMultiSelectedSell;
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
    }

    public void OnElementButtonDown(int index) 
    {
        MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
        if (mainGameUI == null)
        {
            Debug.PrintWarn("未找到ItemElement");
            return;
        }

        if (!selectedItemIndexs.Contains(index)) 
        {
            if (mainGameUI.elements[index].item != null && mainGameUI.elements[index].item.isSellable 
                && GameManager.instance.itemManager.IsCanRemove(mainGameUI.elements[index].item)) 
            {
                mainGameUI.elements[index].SetMultiSelected(true);
                selectedItemIndexs.Add(index);
                GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_1);
            }
        }
        else 
        {
            mainGameUI.elements[index].SetMultiSelected(false);
            selectedItemIndexs.Remove(index);
            GameManager.instance.soundManager.PlaySound(SoundEnum.sound_bubble_2);
        }

        SetMultiButton();
    }

    public void OnGetRecipeClick() 
    {
        List<ItemBaseResource> canBeRandomItems = new List<ItemBaseResource>();
        HashSet<ItemIndex> waitUnlockRecipe = GameManager.instance.unlockRecipe.GetWaitUnlockRecipe();
        foreach (var KV in GameManager.instance.itemConfig.config)
        {
            if (waitUnlockRecipe.Contains(KV.Value.index))
            {
                canBeRandomItems.Add(KV.Value);
            }
        }

        int rnd = GameManager.instance.randomManager.GetRange(RandomType.RandomItem, 0, canBeRandomItems.Count);
        ItemBaseResource randomItem = canBeRandomItems[rnd].Clone();
        GameManager.instance.unlockRecipe.AddUnlockRecipe(randomItem.index);
        GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - refreshHeldCost);

        refreshHeldCount++;
        SetGetRecipeButton();

        MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
        if (mainGameUI != null) 
        {
            GameManager.instance.uiManager.StartFlyItem(randomItem, getRecipeButton.GlobalPosition, mainGameUI.bluePrintFlyTargetNode.GlobalPosition, 0.3);
        }

        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_get_money);
    }
}
