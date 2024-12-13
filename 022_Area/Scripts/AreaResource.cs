using Godot;
using System;

[GlobalClass]
public partial class AreaResource : Resource
{
	[Export] public AreaIndex index;
	[Export] public Texture2D texture;
}
