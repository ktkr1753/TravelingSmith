using Godot;
using System;

[GlobalClass]
public partial class UICommonSettingResource : Resource
{
    [Export] public Color normalColor = new Color(1, 1, 1, 1);
    [Export] public Color betterColor = new Color(0, 1, 0, 1);
    [Export] public Color worseColor = new Color(1, 0, 0, 1);
    [Export] public Color warningCountColor = new Color(1, 1, 0, 1);
}
