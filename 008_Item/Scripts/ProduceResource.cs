using Godot;
using System;

[GlobalClass]
public partial class ProduceResource : ItemBaseResource, IClone<ProduceResource>
{
    private ProduceType _type;
    [Export]
    public ProduceType type
    {
        get { return _type; }
        set { _type = value; }
    }

    private Godot.Collections.Array<ProduceParameter> _parameters = new Godot.Collections.Array<ProduceParameter>();
    [Export] public Godot.Collections.Array<ProduceParameter> parameters
    {
        get { return _parameters; }
        set { _parameters = value; }
    }

    private int _nowParameterIndex = 0;
    [Export] public int nowParameterIndex
    {
        get { return _nowParameterIndex; }
        set 
        { 
            if(_nowParameterIndex != value) 
            {
                int preState = _nowParameterIndex;
                _nowParameterIndex = value;
                OnParameterIndexChange(preState, _nowParameterIndex);
                onParameterIndexChange?.Invoke(preState, _nowParameterIndex);
            }
        }
    }

    private void OnParameterIndexChange(int preState, int nextState) 
    {
        if(0 <= preState && preState < parameters.Count) 
        {
            if (parameters[preState] is IMake make) 
            {
                make.isCostMaterial = false;
            }
        }

        nowTime = 0;
    }

    public Action<int, int> onParameterIndexChange;

    public ProduceParameter nowParameter 
    {
        get 
        {
            return parameters[nowParameterIndex];
        }
    }

    private double _nowTime = 0;
    [Export]
    public double nowTime
    {
        get { return _nowTime; }
        set { _nowTime = value; }
    }

    private int _durability = -1;
    [Export]
    public int durability
    {
        get { return _durability; }
        set
        {
            if (_durability != value)
            {
                _durability = value;
                onDurabilityChange?.Invoke(_durability);
            }
        }
    }

    [Export] private bool _isKeepProduce = true;
    public bool isKeepProduce
    {
        get { return _isKeepProduce; }
        set
        {
            if (_isKeepProduce != value)
            {
                _isKeepProduce = value;
            }
        }
    }

    private bool _isProducing = false;
    [Export]
    public bool isProducing
    {
        get { return _isProducing; }
        private set
        {
            _isProducing = value;
            onIsProducingChange?.Invoke(_isProducing);
        }
    }

    public event Action<bool> onIsProducingChange;
    public event Action<int> onDurabilityChange;

    public void StartProduce()
    {
        isProducing = true;
        GameManager.instance.itemManager.AddProducingItem(this);
    }
    public void StopProduce()
    {
        isProducing = false;
        GameManager.instance.itemManager.RemoveProducingItem(this);
    }

    public override ProduceResource Clone()
    {
        ProduceResource result = base.Clone() as ProduceResource;
        result.type = type;
        result.parameters = parameters.Clone();
        result.nowParameterIndex = nowParameterIndex;
        result.nowTime = nowTime;
        result.durability = durability;
        result.isKeepProduce = isKeepProduce;
        result.isProducing = isProducing;
        return result;
    }

    public override ProduceResource CreateInstanceForClone()
    {
        return new ProduceResource();
    }
}
