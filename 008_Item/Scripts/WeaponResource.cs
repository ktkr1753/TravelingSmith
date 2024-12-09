using Godot;
using System;

[GlobalClass]
public partial class WeaponResource : ItemBaseResource, IClone<WeaponResource>, IAttack
{
    private int _attackPoint = 0;
    [Export] public int attackPoint 
    {
        get { return _attackPoint; }
        set { _attackPoint = value; }
    }

    private double _range = 0;

    [Export] public double range 
    {
        get { return _range; }
        set { _range = value; }
    }

    private FXEnum _fx;
    [Export] public FXEnum fx 
    {
        get { return _fx; }
        private set { _fx = value; }
    }

    private int _durability = 0;
    [Export]
    public int durability
    {
        get { return _durability; }
        set 
        { 
            if(_durability != value) 
            {
                _durability = value;
                onDurabilityChange?.Invoke(_durability);
                if(_durability == 0) 
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
            if(_isUsing != value) 
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
        GameManager.instance.itemManager.AddAttackingItem(this);
    }
    public void StopUsing()
    {
        isUsing = false;
        GameManager.instance.itemManager.RemoveAttackingItem(this);
    }


    public override WeaponResource Clone()
    {
        WeaponResource result = base.Clone() as WeaponResource;
        result.attackPoint = attackPoint;
        result.range = range;
        result.fx = fx;
        result.durability = durability;
        result.needTime = needTime;
        result.nowTime = nowTime;
        result.isUsing = isUsing;

        return result;
    }

    public override WeaponResource CreateInstanceForClone()
    {
        return new WeaponResource();
    }
}
