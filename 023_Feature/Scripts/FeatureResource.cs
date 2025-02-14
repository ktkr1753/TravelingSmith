using Godot;
using System;

[GlobalClass]
public partial class FeatureResource : Resource
{
	[Export] public FeatureIndex index;
	[Export] public Texture2D texture;
	[Export] public Godot.Collections.Array<FeatureContentIndex> goodEffect = new Godot.Collections.Array<FeatureContentIndex>();
	[Export] public Godot.Collections.Array<FeatureContentIndex> badEffect = new Godot.Collections.Array<FeatureContentIndex>();
}
