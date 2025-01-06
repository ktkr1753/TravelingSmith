using Godot;
using System;

[GlobalClass]
public partial class MapAttackObjectConfigResource : Resource
{
	[Export] public Godot.Collections.Dictionary<ItemIndex, PackedScene> config = new Godot.Collections.Dictionary<ItemIndex, PackedScene>(); 
}
