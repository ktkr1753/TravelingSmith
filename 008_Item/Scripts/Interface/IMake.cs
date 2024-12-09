using Godot;
using System;

public partial interface IMake : IProduce
{
    public Godot.Collections.Array<ItemIndex> materials { get; }
    public bool isCostMaterial { get; set; }
}
