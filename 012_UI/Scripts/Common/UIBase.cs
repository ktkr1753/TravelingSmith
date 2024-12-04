using Godot;
using System;
using System.Collections.Generic;

public partial class UIBase : Control
{
    [Export] public bool isSingle = true;
    [Export] public bool isBackGround = true;
    [Export] public bool isFoucs = true;
    [Export] public UIIndex uIType = UIIndex.None;

    public List<object> parameters = new List<object>();

    public void SetParameter(List<object> parameters)
    {
        this.parameters = parameters;
    }

    public virtual void Init()
    {

    }
}
