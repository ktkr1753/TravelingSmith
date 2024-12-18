using Godot;
using System;

public partial class ShopObject : Node2D
{
    private int _index;
    public int index 
    {
        get 
        {
            return _index;
        }
        private set 
        {
            _index = value;
        }
    }

    public void SetIndex(int index) 
    {
        this.index = index;
    }
}
