using Godot;
using System;

public partial interface IProduce
{
    public ItemIndex productItem { get; set; }

    public double needTime { get; set; }
}
