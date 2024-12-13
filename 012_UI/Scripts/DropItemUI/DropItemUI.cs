using Godot;
using System;
using System.Collections.Generic;

public partial class DropItemUI : UIBase
{
	[Export] public Filter filter;
    [Export] public Control mask;

    public override void Init()
    {
        base.Init();

        if(parameters.Count > 1) 
        {
            GameManager.instance.isChoosePause = true;
            Control view = parameters[0] as Control;
            List<Control> interacts = parameters[1] as List<Control>;

            SetData(view, interacts);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        GameManager.instance.isChoosePause = false;
    }

    public void SetData(Control view, List<Control> interacts) 
    {
        SetView(view);
        SetFilter(interacts);
    }


    private void SetView(Control view) 
    {
        mask.GlobalPosition = view.GlobalPosition;
        mask.Size = view.Size;
    }

    private void SetFilter(List<Control> interacts) 
    {
        FilterInfo info = new FilterInfo();

        info.canUseUIs.AddRange(interacts);

        filter.info = info;
    }


}
