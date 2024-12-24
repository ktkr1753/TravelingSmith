using Godot;
using System;

[GlobalClass]
public partial class EngineResource : ItemBaseResource, IClone<EngineResource>, ICore
{
    [Export] private double _accelration;
    public double acceleration 
    {
        get { return _accelration; }
        set { _accelration = value; }
    }

    [Export] private double _maxSpeed;
    public double maxSpeed 
    {
        get { return _maxSpeed; }
        set { _maxSpeed = value; }
    }

    private bool _isUsing = false;
    [Export]
    public bool isUsing
    {
        get { return _isUsing; }
        private set
        {
            _isUsing = value;
        }
    }

    public void StartCore()
    {
        isUsing = true;
        GameManager.instance.itemManager.AddCoreingItem(this);
    }
    public void StopCore()
    {
        isUsing = false;
        GameManager.instance.itemManager.RemoveCoreingItem(this);
    }

    public override EngineResource Clone()
    {
        EngineResource result = base.Clone() as EngineResource;
        result.acceleration = acceleration;
        result.maxSpeed = maxSpeed;

        return result;
    }

    public override EngineResource CreateInstanceForClone()
    {
        return new EngineResource();
    }
}
