using Godot;
using System;

[GlobalClass]
public partial class FeatureConfigResource : Resource
{
	[Export] public Godot.Collections.Dictionary<FeatureIndex, FeatureResource> config = new Godot.Collections.Dictionary<FeatureIndex, FeatureResource>(); 
}
