using Godot;
using System;
using System.Collections.Generic;

public partial class PickUI : UIBase
{
    [Export] ItemElementPool itemElementPool;
    [Export] ItemInfoPanel itemInfoPanel;
    [Export] Button confirmButton;
    private List<ItemBaseResource> items = new List<ItemBaseResource>();

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

    public override void Init()
    {
        base.Init();

        if (parameters.Count > 0) 
        {
            GameManager.instance.isChoosePause = true;
            items = parameters[0] as List<ItemBaseResource>;
            SetView();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        GameManager.instance.isChoosePause = false;
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
        }
    }

    private void ClearItems() 
    {
        for (int i = 0; i < itemElementPool.inuses.Count; i++)
        {
            ItemElement element = itemElementPool.inuses[i];
            element.SetData(-1, null);
            element.onMainButtonDown -= OnItemButtonDown;
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
            confirmButton.Disabled = true;
        }
    }



    private void OnItemButtonDown(int index) 
    {
        nowSelectedItemIndex = index;
    }

    public void OnConfirmClick() 
    {
        GameManager.instance.uiManager.CloseUI(this);

        ItemElement itemElement = itemElementPool.inuses[nowSelectedItemIndex];

        Vector2 startPoint = itemElement.GlobalPosition;

        int putIndex = GameManager.instance.itemManager.AddHeldItem(itemElement.item.index);

        Vector2 endPoint = Vector2.Zero;
        MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
        if (mainGameUI != null) 
        {
            mainGameUI.elements[putIndex].isFlying = true;
            endPoint = mainGameUI.elements[putIndex].GlobalPosition;
        }

        GameManager.instance.uiManager.StartFlyItem(itemElement.item, startPoint, endPoint, 0.3, () =>
        {
            mainGameUI.elements[putIndex].isFlying = false;
        });
    }

    public void OnCancelClick() 
    {
        GameManager.instance.uiManager.CloseUI(this);
    }
}
