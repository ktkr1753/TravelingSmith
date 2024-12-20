using Godot;
using System;
using System.Collections.Generic;

public partial class PickUI : UIBase
{
    [Export] ItemElementPool itemElementPool;
    [Export] ItemInfoPanel itemInfoPanel;
    [Export] Button confirmButton;
    private List<ItemBaseResource> items = new List<ItemBaseResource>();
    private Action pauseFinishCallback;

    private int _nowSelectedItemIndex = -1;
    public int nowSelectedItemIndex 
    {
        get { return _nowSelectedItemIndex; }
        set 
        {
            if (_nowSelectedItemIndex != value) 
            {
                int preState = _nowSelectedItemIndex;
                _nowSelectedItemIndex = value;
                OnSelectedIndexChange(preState, _nowSelectedItemIndex);
            }
        }
    }

    private void OnSelectedIndexChange(int preState, int nextState) 
    {
        if(preState >= 0 && preState < itemElementPool.inuses.Count) 
        {
            itemElementPool.inuses[preState].nowSelectedState = ItemElement.SelectedState.None;
        }

        if(nextState >= 0 && nextState < itemElementPool.inuses.Count) 
        {
            itemElementPool.inuses[nextState].nowSelectedState = ItemElement.SelectedState.Selected;
            SetInfoPanel(itemElementPool.inuses[nextState].item);
        }
        else 
        {
            SetInfoPanel(null);
        }
        SetConfirmButton();
    }

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

                if (preState >= 0 && preState < itemElementPool.inuses.Count)
                {
                    itemElementPool.inuses[preState].isPicking = false;
                }

                if (_nowPickElementIndex >= 0 && _nowPickElementIndex < itemElementPool.inuses.Count)
                {
                    SetPickedItemElement(itemElementPool.inuses[_nowPickElementIndex].item);
                    itemElementPool.inuses[_nowPickElementIndex].isPicking = true;
                }
                else
                {
                    SetPickedItemElement(null);
                }
            }
        }
    }


    public override void Init()
    {
        base.Init();

        if (parameters.Count > 0) 
        {
            //GameManager.instance.isChoosePause = true;
            pauseFinishCallback = GameManager.instance.AddNeedPause();
            items = parameters[0] as List<ItemBaseResource>;
            SetView();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        pauseFinishCallback?.Invoke();
        pauseFinishCallback = null;
    }


    private void SetView() 
    {
        SetItems();
        SetConfirmButton();
    }


    private void SetItems() 
    {
        ClearItems();
        for (int i = 0; i < items.Count; i++) 
        {
            ItemElement element = itemElementPool.GetElement();
            element.SetData(i, items[i]);
            element.onMainButtonDown += OnItemButtonDown;
            element.onMainButtonUp += OnItemButtonUp;
        }
    }

    private void ClearItems() 
    {
        for (int i = 0; i < itemElementPool.inuses.Count; i++)
        {
            ItemElement element = itemElementPool.inuses[i];
            element.SetData(-1, null);
            element.onMainButtonDown -= OnItemButtonDown;
            element.onMainButtonUp -= OnItemButtonUp;
        }

        itemElementPool.ReturnAllElement();
    }

    private void SetInfoPanel(ItemBaseResource item)
    {
        if (item != null)
        {
            itemInfoPanel.Visible = true;
            itemInfoPanel.SetData(item);
        }
        else
        {
            itemInfoPanel.Visible = false;
        }
    }

    private void SetConfirmButton() 
    {
        if(nowSelectedItemIndex != -1 && !GameManager.instance.itemManager.isHeldItemFull) 
        {
            confirmButton.Disabled = false;
        }
        else 
        {
            confirmButton.Disabled = false;
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

    private void OnItemButtonDown(int index) 
    {
        nowSelectedItemIndex = index;

        nowPickElementIndex = index;

        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
    }

    private void OnItemButtonUp(int index) 
    {
        ItemBaseResource pickedItem = null;
        ItemElement element;
        if (nowPickElementIndex != -1)
        {
            element = itemElementPool.inuses[nowPickElementIndex];
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
                    if (GameManager.instance.itemManager.heldItems[mainGameUI.nowEnterElementIndex] == null)
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
                        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
                        GameManager.instance.uiManager.CloseUI(this);
                    }
                }
            }
        }
        nowPickElementIndex = -1;
    }

    public void OnConfirmClick() 
    {
        ItemElement itemElement = itemElementPool.inuses[nowSelectedItemIndex];

        if (GameManager.instance.itemManager.isHeldItemFull) 
        {
            GameManager.instance.uiManager.CloseUI(this);

            GameManager.instance.itemManager.waitAddItem = itemElement.item;
            MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
            if(mainGameUI != null) 
            {
                List<Control> interacts = new List<Control>();
                for(int i = 0; i < mainGameUI.elements.Count; i++) 
                {
                    interacts.Add(mainGameUI.elements[i].mainButton);
                }

                DropItemUI dropItemUI = GameManager.instance.uiManager.OpenUI<DropItemUI>(UIIndex.DropItemUI, new List<object>()
                {
                    mainGameUI.itemsPanel,
                    interacts
                });
            }
        }
        else 
        {
            GameManager.instance.uiManager.CloseUI(this);

            Vector2 startPoint = itemElement.GlobalPosition;
            int putIndex = GameManager.instance.itemManager.AddHeldItem(itemElement.item.index);
            Vector2 endPoint = Vector2.Zero;
            MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
            if (mainGameUI != null) 
            {
                mainGameUI.elements[putIndex].isFlying = true;
                endPoint = mainGameUI.elements[putIndex].GlobalPosition;
                GameManager.instance.uiManager.StartFlyItem(itemElement.item, startPoint, endPoint, 0.3, () =>
                {
                    mainGameUI.elements[putIndex].isFlying = false;
                });
            }
        }
    }

    public void OnCancelClick() 
    {
        GameManager.instance.uiManager.CloseUI(this);
    }
}
