using Godot;
using System;

public partial interface IUseable
{
    public int durability { get; set; }

    public bool isUsing { get; }

    public double needTime { get; set; }

    public double nowTime { get; set; }

    public event Action<bool> onIsUsingChange;
    public event Action<int> onDurabilityChange;
    public event Action onUseUp;

    public void StartUsing();

    public void StopUsing();
}
