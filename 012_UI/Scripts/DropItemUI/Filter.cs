using Godot;
using System;

public partial class Filter : Control
{
    private FilterInfo _info = new FilterInfo();
    public FilterInfo info
    {
        get { return _info; }
        set
        {
            if (value != null)
            {
                _info = value;
            }
            else
            {
                _info = new FilterInfo();
            }
        }
    }


    public override void _Input(InputEvent @event)
    {
        bool isCannotUse = CheckIsCannotUseUI(@event);
        bool isCanUseButton = CheckBaseButton(@event);
        bool isCanUseScrollContainer = CheckScrollContainer(@event);

        if(!isCannotUse && (isCanUseButton || isCanUseScrollContainer)) 
        {
            MouseFilter = MouseFilterEnum.Ignore;
        }
        else 
        {
            MouseFilter = MouseFilterEnum.Stop;
        }
    }

    private bool CheckIsCannotUseUI(InputEvent @event) 
    {
        bool result = IsEventInCannotUseUI(@event);

        return result;
    }


    private bool CheckBaseButton(InputEvent @event)
    {
        bool isCanUse = false;
        for (int i = 0; i < info.canUseUIs.Count; i++)
        {
            if (info.canUseUIs[i] is BaseButton baseButton)
            {
                Rect2 buttonRect = new Rect2(baseButton.GlobalPosition, baseButton.Size);
                if (@event is InputEventMouseButton mouseButton)
                {
                    if (buttonRect.HasPoint(mouseButton.GlobalPosition))
                    {
                        isCanUse = true;
                        break;
                    }
                }
                else if (@event is InputEventMouseMotion mouseMotion)
                {
                    if (buttonRect.HasPoint(mouseMotion.GlobalPosition))
                    {
                        isCanUse = true;
                        break;
                    }
                }
            }
        }

        return isCanUse;
    }

    private bool CheckScrollContainer(InputEvent @event)
    {
        bool isCanUse = false;
        for (int i = 0; i < info.canUseUIs.Count; i++)
        {
            if (info.canUseUIs[i] is ScrollContainer scroll)
            {
                Rect2 scrollRect = new Rect2(scroll.GlobalPosition, scroll.Size);
                if (@event is InputEventMouseButton mouseButton)
                {
                    if (scrollRect.HasPoint(mouseButton.GlobalPosition))
                    {
                        isCanUse = true;
                        break;
                    }
                }
                else if (@event is InputEventMouseMotion mouseMotion)
                {
                    if (scrollRect.HasPoint(mouseMotion.GlobalPosition))
                    {
                        isCanUse = true;
                        break;
                    }
                }
            }
        }

        return isCanUse;
    }

    private bool IsEventInCannotUseUI(InputEvent @event)
    {
        bool result = false;

        for (int i = 0; i < info.cannotUseUIs.Count; i++)
        {
            Rect2 cannotUseRect = new Rect2(info.cannotUseUIs[i].GlobalPosition, info.cannotUseUIs[i].Size);
            if (@event is InputEventMouseButton mouseButton)
            {
                if (cannotUseRect.HasPoint(mouseButton.GlobalPosition))
                {
                    result = true;
                    break;
                }
            }
            else if (@event is InputEventMouseMotion mouseMotion)
            {
                if (cannotUseRect.HasPoint(mouseMotion.GlobalPosition))
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }
}
