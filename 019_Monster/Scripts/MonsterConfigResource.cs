using Godot;
using System;

[GlobalClass]
public partial class MonsterConfigResource : Resource
{
	[Export] public Godot.Collections.Dictionary<MonsterIndex, MonsterResource> config = new Godot.Collections.Dictionary<MonsterIndex, MonsterResource>();
}
