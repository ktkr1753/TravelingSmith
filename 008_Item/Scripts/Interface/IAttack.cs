using Godot;
using System;

public partial interface IAttack: IUseable
{
	public int attackPoint { get; set; }
	public double range { get; set; }

	public FXEnum fx { get; }

    public string sound { get; }
}
