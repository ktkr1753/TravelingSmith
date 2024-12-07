using Godot;
using System;

public partial interface IProduce
{
    public ItemIndex productItem { get; set; }
    public double needTime { get; set; }
    public double nowTime { get; set; }

    public bool isKeepProduce { get; }
    public bool isProducing { get; }

    public event Action<bool> onIsProducingChange;

    public void StartProduce();
    public void StopProduce();
}
