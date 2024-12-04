using Godot;
using System;

[GlobalClass]
public partial class UIConfigResource : Resource
{
    [Export] public PackedScene prefab;
    [Export] public UILayer layer;
}
