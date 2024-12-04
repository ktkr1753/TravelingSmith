using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class MaterialConfig : Resource
{
    [Export] public Godot.Collections.Dictionary<MaterialIndex, Material> config = new Godot.Collections.Dictionary<MaterialIndex, Material>();
}
