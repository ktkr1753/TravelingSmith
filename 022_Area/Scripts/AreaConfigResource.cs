using Godot;
using System;

[GlobalClass]
public partial class AreaConfigResource : Resource
{
	[Export] public Godot.Collections.Dictionary<AreaIndex, AreaResource> config = new Godot.Collections.Dictionary<AreaIndex, AreaResource>();
}
