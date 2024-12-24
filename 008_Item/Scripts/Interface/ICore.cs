using Godot;
using System;

public partial interface ICore
{
    public double acceleration { get; set; }

    public double maxSpeed { get; set; }

    public bool isUsing { get; }

    public void StartCore();
    public void StopCore();
}
