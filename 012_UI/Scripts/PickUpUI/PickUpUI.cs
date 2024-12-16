using Godot;
using System;

public partial class PickUpUI : UIBase
{
	[Export] private ItemElement pickUpElement;

    private ItemBaseResource item;

    public override void Init()
    {
        base.Init();

        if (parameters.Count > 0 && parameters[0] is ItemBaseResource item) 
        {
            SetData(item);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdateSelectedItemImagePos();
    }

    private void UpdateSelectedItemImagePos()
    {
        pickUpElement.GlobalPosition = GetViewport().GetMousePosition() - new Vector2(pickUpElement.Size.X / 2, pickUpElement.Size.Y / 2);
    }

    public void SetData(ItemBaseResource item) 
    {
        this.item = item;

        SetView();
    }


    private void SetView() 
    {
        pickUpElement.SetData(item);
        UpdateSelectedItemImagePos();
    }



}
