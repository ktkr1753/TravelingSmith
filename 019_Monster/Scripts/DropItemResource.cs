using Godot;
using System;

[GlobalClass]
public partial class DropItemResource : Resource
{
	[Export] public ItemIndex itemIndex;
	[Export(PropertyHint.Range, "0,1,0.05")] public float dropRate;
}
