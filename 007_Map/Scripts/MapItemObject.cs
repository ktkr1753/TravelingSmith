using Godot;
using System;

public partial class MapItemObject : Node2D
{
    [Export] public Sprite2D image;
    [Export] public Area2D area;
    [Export] public Material outLineMaterial;
    private ItemBaseResource _item;
    public ItemBaseResource item 
    {
        get { return _item; }
        private set { _item = value; }
    }

    private bool _isPicked = false;
    public bool isPicked
    {
        get { return _isPicked; }
    }

    public bool _isTouched = false;
    public bool isTouched 
    {
        get { return _isTouched; }
        set { _isTouched = value; }
    }

    public event Action<MapItemObject> OnNeedReturn;

    public Vector2 imageFix = new Vector2(-12, -12);

    public void SetData(ItemBaseResource item)
    {
        this.item = item;
        ResetSetIsTouched();
        SetArea();
        SetView();
    }

    private void ResetSetIsTouched() 
    {
        if(item != null) 
        {
            isTouched = false;
        }
    }

    public void SetView()
    {
        if(item != null) 
        {
            image.Texture = item.texture;
        }
        else 
        {
            image.Texture = null;
        }
    }

    public void SetArea() 
    {
        if(item != null) 
        {
            area.Monitorable = true;
            area.InputPickable = true;
        }
        else 
        {
            area.Monitorable = false;
            area.InputPickable = false;
        }
    }

    public void OnInputEvent(Node viewport, InputEvent inputEvent, int shapeIdx) 
    {
        if(item == null) 
        {
            return;
        }

        if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.Pressed)
        {
            //Debug.Print("OnInputEvent Pressed ");
            int addIndex = GameManager.instance.itemManager.AddHeldItem(item.index);
            if (addIndex != -1) 
            {
                MainGameUI mainGameUI = GameManager.instance.uiManager.GetOpenedUI<MainGameUI>(UIIndex.MainGameUI);
                if(mainGameUI == null) 
                {
                    return;
                }

                Vector2 startPos = GetGlobalTransformWithCanvas().Origin + imageFix;
                mainGameUI.elements[addIndex].isFlying = true;
                Vector2 endPos = mainGameUI.elements[addIndex].GlobalPosition;

                GameManager.instance.uiManager.StartFlyItem(item, startPos, endPos, 0.3, () => 
                {
                    mainGameUI.elements[addIndex].isFlying = false;
                });

                OnNeedReturn?.Invoke(this);
            }
        }
    }

    public void OnMouseEnter() 
    {
        image.Material = outLineMaterial;
    }

    public void OnMouseExit() 
    {
        image.Material = null;
    }
}
