using Godot;
using System;

[Flags]
public enum AreaIndex
{
    None = 0,
    Normal = 1 << 0,    //1
    Produce = 1 << 1,   //2
    Useable = 1 << 2,   //4
    Core = 1 << 3,      //8
}
