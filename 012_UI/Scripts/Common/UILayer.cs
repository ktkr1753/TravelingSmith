using Godot;
using System;

public enum UILayer
{
	None = 0,
    Layer1 = 1,     //最上層，不被black panel蓋住
    Layer2 = 2,     //系統層，高於layer3
    Layer3 = 3,		//普通層
}
