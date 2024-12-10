using Godot;
using System;
using System.Collections.Generic;

public partial class ShopUI : UIBase
{
    public enum PositionState 
    {
        None = 0,
        In = 1,
        MovingIn = 2,
        Out = 3,
        MovingOut = 4,
    }

    [Export] private AnimationPlayer animation;
    [Export] private NinePatchRect moveButtonImage;
    [Export] private TextureRect arrorImage;
    [Export] private Godot.Collections.Dictionary<PositionState, Texture2D> moveButtonTextures = new Godot.Collections.Dictionary<PositionState, Texture2D>();
    [Export] private Godot.Collections.Dictionary<PositionState, Texture2D> arrorTextures = new Godot.Collections.Dictionary<PositionState, Texture2D>();
    [Export] private ShopItemElementPool itemPool;

    private PositionState _positionState = PositionState.Out;
    public PositionState positionState 
    {
        get { return _positionState; }
        set 
        {
            if(_positionState != value) 
            {
                _positionState = value;
                OnPositionStateChange(_positionState);
            }
        }
    }

    private void OnPositionStateChange(PositionState nowPositionState) 
    {
        SetMoveButtonImage();
        SetArrowImage();
    }

	public List<ItemBaseResource> sellItems = new List<ItemBaseResource>();

    public const double flyTime = 0.2;

    public const string clip_moveIn = "moveIn";
    public const string clip_moveOut = "moveOut";

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
        SetMoveButtonImage();
        SetArrowImage();
        SetShopItemElements();
    }

    private void SetMoveButtonImage() 
    {
        if(moveButtonTextures.TryGetValue(positionState, out Texture2D texture)) 
        {
            moveButtonImage.Texture = texture;
        }
        else 
        {
            Debug.PrintErr($"沒有moveButtonTexture, positionState:{positionState}");
        }
    }
    private void SetArrowImage()
    {
        if (arrorTextures.TryGetValue(positionState, out Texture2D texture))
        {
            arrorImage.Texture = texture;
        }
        else
        {
            Debug.PrintErr($"沒有arrorTexture, positionState:{positionState}");
        }
    }

    private void SetShopItemElements() 
    {
        ClearShopItemElements();
        for (int i = 0; i < sellItems.Count; i++) 
        {
            ShopItemElement element = itemPool.GetElement();
            element.SetData(i, sellItems[i]);
            element.itemElement.onMainButtonDown += OnShopItemButtonDown;
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
    }

    private void OnShopItemButtonDown(int index) 
    {
        Debug.Print($"OnShopItemButtonDown index:{index}");

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
                int addIndex = GameManager.instance.itemManager.AddHeldItem(element.item.index);
                if(addIndex != -1) 
                {
                    //資料
                    sellItems.Remove(element.item);
                    GameManager.instance.itemManager.money = Math.Max(0, GameManager.instance.itemManager.money - element.money);
                    //演出
                    MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
                    if (mainGameUI != null) 
                    {
                        mainGameUI.elements[addIndex].isFlying = true;
                        Vector2 endPosition = mainGameUI.elements[addIndex].GlobalPosition;
                        GameManager.instance.uiManager.StartFlyItem(element.item, element.GlobalPosition, endPosition, flyTime, () =>
                        {
                            mainGameUI.elements[addIndex].isFlying = false;
                        });
                    }

                    ClearShopItemElementData(element);
                    itemPool.ReturnElement(element);
                }
            }
        }
    }

    public async void OnMoveClick() 
    {
        switch (positionState) 
        {
            case PositionState.Out: 
                {
                    GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
                    positionState = PositionState.MovingIn;
                    animation.Play(clip_moveIn);
                    await ToSignal(animation, "animation_finished").ToTask();
                    positionState = PositionState.In;
                }
                break;
            case PositionState.In: 
                {
                    GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
                    positionState = PositionState.MovingOut;
                    animation.Play(clip_moveOut);
                    await ToSignal(animation, "animation_finished").ToTask();
                    positionState = PositionState.Out;
                }
                break;
        }
    }


}
