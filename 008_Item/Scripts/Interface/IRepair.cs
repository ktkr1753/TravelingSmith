using Godot;
using System;

public partial interface IRepair: IUseable
{
    public int repairPoint { get; set; }
}
