using Godot;
using System;

public partial interface IClone<T>
{
    public T Clone();

    public T CreateInstanceForClone();
}
