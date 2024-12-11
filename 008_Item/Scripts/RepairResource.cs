using Godot;
using System;

[GlobalClass]
public partial class RepairResource : ItemBaseResource, IClone<RepairResource>, IRepair
{
    private int _repairPoint = 0;
    [Export] public int repairPoint
    {
        get { return _repairPoint; }
        set { _repairPoint = value; }
    }

    private int _durability = 0;
    [Export] public int durability
    {
        get { return _durability; }
        set
        {
            if (_durability != value)
            {
                _durability = value;
                onDurabilityChange?.Invoke(_durability);
                if (_durability == 0)
                {
                    onUseUp?.Invoke();
                }
            }
        }
    }

    private double _needTime = 0;
    [Export] public double needTime
    {
        get { return _needTime; }
        set { _needTime = value; }
    }
    private double _nowTime = 0;
    [Export] public double nowTime
    {
        get { return _nowTime; }
        set { _nowTime = value; }
    }
    private bool _isUsing = false;
    [Export] public bool isUsing
    {
        get { return _isUsing; }
        private set
        {
            if (_isUsing != value)
            {
                _isUsing = value;
                onIsUsingChange?.Invoke(_isUsing);
            }
        }
    }

    public event Action<bool> onIsUsingChange;
    public event Action<int> onDurabilityChange;
    public event Action onUseUp;

    public void StartUsing()
    {
        isUsing = true;
        GameManager.instance.itemManager.AddRepairingItem(this);
    }
    public void StopUsing()
    {
        isUsing = false;
        GameManager.instance.itemManager.RemoveRepairingItem(this);
    }

    public override RepairResource Clone()
    {
        RepairResource result = base.Clone() as RepairResource;
        result.repairPoint = repairPoint;
        result.durability = durability;
        result.needTime = needTime;
        result.nowTime = nowTime;
        result.isUsing = isUsing;

        return result;
    }

    public override RepairResource CreateInstanceForClone()
    {
        return new RepairResource();
    }
}
