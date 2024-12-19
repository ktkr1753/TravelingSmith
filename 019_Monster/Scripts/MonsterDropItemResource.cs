using Godot;
using System;

[GlobalClass]
public partial class MonsterDropItemResource : Resource
{
	[Export] public ItemIndex itemIndex;
	[Export(PropertyHint.Range, "0,1,0.05")] public float dropRate;
}
