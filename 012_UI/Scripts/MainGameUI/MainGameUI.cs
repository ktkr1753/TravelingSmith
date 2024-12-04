using Godot;
using System;

public partial class MainGameUI : UIBase
{
	[Export] Godot.Collections.Array<MainGameItemElement> elements = new Godot.Collections.Array<MainGameItemElement>();
    [Export] private TextureRect pickedItemImage;
    [Export] private ItemInfoPanel infoPanel;

	public Godot.Collections.Array<ItemBaseResource> items = new Godot.Collections.Array<ItemBaseResource>();

    //碰到的物件ID
    private int _nowEnterElementIndex = -1;
    public int nowEnterElementIndex 
    {
        get { return _nowEnterElementIndex; }
        set 
        {
            if (_nowEnterElementIndex != value) 
            {
                _nowEnterElementIndex = value;
                SetItemSelectedState(nowEnterElementIndex, nowSelectedElementIndex);
            }
        }
    }

    //點選的物件ID
    private int _nowSelectedElementIndex = -1;

    public int nowSelectedElementIndex 
    {
        get { return _nowSelectedElementIndex; }
        set 
        {
            if (_nowSelectedElementIndex != value) 
            {
                //Debug.Print($"nowSelectedElementIndex change:{value}");
                _nowSelectedElementIndex = value;
                SetItemSelectedState(nowEnterElementIndex, nowSelectedElementIndex);
            }
        }
    }

    private Vector2? recordClickPos = null;

    //拉動的物件ID
    private int _nowPickElementIndex = -1;
    public int nowPickElementIndex 
    {
        get { return _nowPickElementIndex; }
        set 
        {
            if(_nowPickElementIndex != value) 
            {
                //RefreshItemElement(_nowPickElementIndex, items[_nowPickElementIndex]);
                _nowPickElementIndex = value;
                if(_nowPickElementIndex != -1) 
                {
                    SetPickedItemImageImage(items[_nowPickElementIndex]);
                    RefreshItemElement(_nowPickElementIndex, null);
                }
                else 
                {
                    SetPickedItemImageImage(null);
                }
            }

            _nowPickElementIndex = value; 
        }
    }

    public const float moveThreshold = 5;


    public override void Init()
    {
        base.Init();
        InitElements();

        //測試
        for (int i = 0; i < 25; i++) 
        {
            if(i == 0 || i == 2) 
            {
                items.Add(GameManager.instance.itemConfig.config[ItemIndex.Iron]);
            }
            else if (i == 24)
            {
                items.Add(GameManager.instance.itemConfig.config[ItemIndex.RecipeDart]);
            }
            else 
            {
                items.Add(null);
            }
        }
        RefreshItemElements();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        CheckIsPickItem();
        UpdateSelectedItemImagePos();
    }

    private void CheckIsPickItem() 
    {
        if(recordClickPos != null) 
        {
            Vector2 nowPos = GetViewport().GetMousePosition();
            if(recordClickPos.GetValueOrDefault(Vector2.Zero).DistanceTo(nowPos) >= moveThreshold && nowSelectedElementIndex != -1) 
            {
                nowPickElementIndex = nowSelectedElementIndex;
            }
            else 
            {
                nowPickElementIndex = -1;
            }
        }
        else 
        {
            nowPickElementIndex = -1;
        }
    }

    private void InitElements() 
    {
        for(int i = 0; i < elements.Count; i++) 
        {
            elements[i].onMouseEnter += OnElementMouseEnter;
            elements[i].onMouseExit += OnElementMouseExit;
            elements[i].onMainButtonDown += OnElementButtonDown;
            elements[i].onMainButtonUp += OnElementButtonUp;
        }
    }

    public void RefreshItemElements() 
	{
		for(int i = 0; i < elements.Count; i++) 
		{
            if (nowSelectedElementIndex == i) 
            {
                RefreshItemElement(i, null);
            }
            else 
            {
    			RefreshItemElement(i, items[i]);
            }
        }
	}

    public void RefreshItemElement(int index, ItemBaseResource item)
    {
		elements[index].SetData(item);
    }

    private void SetItemSelectedState(int nowEnterElementIndex, int nowSelectedElementIndex)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (i == nowSelectedElementIndex)
            {
                elements[i].nowSelectedState = MainGameItemElement.SelectedState.Selected;
            }
            else if (i == nowEnterElementIndex)
            {
                elements[i].nowSelectedState = MainGameItemElement.SelectedState.Hover;
            }
            else
            {
                elements[i].nowSelectedState = MainGameItemElement.SelectedState.None;
            }
        }
    }

    private void SetPickedItemImageImage(ItemBaseResource item) 
    {
        if (item != null)
        {
            pickedItemImage.Visible = true;
            pickedItemImage.Texture = item.texture;
        }
        else 
        {
            pickedItemImage.Visible = false;
        }
    }

    private void SetInfoPanel(ItemBaseResource item) 
    {
        infoPanel.SetData(item);
    }


    private void UpdateSelectedItemImagePos() 
    {
        if (pickedItemImage.Visible) 
        {
            pickedItemImage.GlobalPosition = GetViewport().GetMousePosition() - new Vector2(pickedItemImage.Size.X / 2, pickedItemImage.Size.Y / 2);
        }
    }

    private void OnElementMouseEnter(int index) 
    {
        if(index != nowEnterElementIndex) 
        {
            nowEnterElementIndex = index;
            //Debug.Print($"OnElementMouseEnter index:{nowEnterElementIndex}");
            SetInfoPanel(items[nowEnterElementIndex]);
        }
    }

    private void OnElementMouseExit(int index) 
    {
        if(index == nowEnterElementIndex) 
        {
            nowEnterElementIndex = -1;
            //Debug.Print($"OnElementMouseExit index:{nowEnterElementIndex}");
        }
    }

    private void OnElementButtonDown(int index) 
    {
        nowSelectedElementIndex = index;

        recordClickPos = GetViewport().GetMousePosition();

        //Debug.Print($"OnElementButtonDown index:{nowSelectedElementIndex}");
    }

    private void OnElementButtonUp(int index) 
    {
        ItemBaseResource pickedItem = null;
        ItemBaseResource putItem = null;
        if (nowPickElementIndex != -1) 
        {
            pickedItem = items[nowPickElementIndex];
        }

        if (nowEnterElementIndex != -1 && pickedItem != null) 
        {
            putItem = items[nowEnterElementIndex];
            items[nowPickElementIndex] = putItem;
            items[nowEnterElementIndex] = pickedItem;
            RefreshItemElement(nowPickElementIndex, items[nowPickElementIndex]);
            RefreshItemElement(nowEnterElementIndex, items[nowEnterElementIndex]);
        }
        else 
        {
            if(nowPickElementIndex != -1) 
            {
                RefreshItemElement(nowPickElementIndex, pickedItem);
            }
        }

        recordClickPos = null;

        if(nowEnterElementIndex != -1)
        {
            nowSelectedElementIndex = nowEnterElementIndex;
        }
        //Debug.Print($"OnElementButtonUp index:{nowSelectedElementIndex}");
    }
}
