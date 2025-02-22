using Godot;
using System;

[GlobalClass]
public partial class MakeParameter : ProduceParameter, IClone<ProduceParameter>, IMake
{
    private Godot.Collections.Array<ItemIndex> _materials = new Godot.Collections.Array<ItemIndex>();
    [Export] public Godot.Collections.Array<ItemIndex> materials 
    {
        get { return _materials; }
        private set { _materials = value; }
    }

    private bool _isCostMaterial = false;
    [Export] public bool isCostMaterial 
    {
        get { return _isCostMaterial; }
        set 
        {
            _isCostMaterial = value; 
        }
    }

    public override MakeParameter Clone()
    {
        MakeParameter result = base.Clone() as MakeParameter;

        result.materials = materials.Clone();
        result.isCostMaterial = isCostMaterial;

        return result;
    }

    public override MakeParameter CreateInstanceForClone()
    {
        return new MakeParameter();
    }
}
