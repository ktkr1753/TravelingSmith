using Godot;
using System;

[GlobalClass]
public partial class ItemConfigResource : Resource
{
	[Export] public Godot.Collections.Dictionary<ItemIndex, ItemBaseResource> config;
}
