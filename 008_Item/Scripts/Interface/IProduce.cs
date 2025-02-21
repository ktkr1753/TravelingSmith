using Godot;
using System;

public partial interface IProduce
{
    public ProduceType type { get; set; }
    public ItemIndex productItem { get; set; }
    public double needTime { get; set; }
    public double nowTime { get; set; }

    public int durability { get; set; }
    public bool isKeepProduce { get; set; }
    public bool isProducing { get; }

    public event Action<bool> onIsProducingChange;
    public event Action<int> onDurabilityChange;

    public void StartProduce();
    public void StopProduce();
}
