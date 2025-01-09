using Godot;
using System;

public partial interface IMake : IProduce
{
    public MakeType type { get; set; }
    public Godot.Collections.Array<ItemIndex> materials { get; }
    public bool isCostMaterial { get; set; }
}
