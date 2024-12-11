using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class ExpConfigResource : Resource
{
	[Export] public Godot.Collections.Array<int> expIntervals = new Godot.Collections.Array<int>();

	private Godot.Collections.Array<int> _expAllIntervals = null;

    public Godot.Collections.Array<int> expAllIntervals 
	{
		get 
		{ 
			if(_expAllIntervals == null) 
			{
				_expAllIntervals = new Godot.Collections.Array<int>();
				int needExp = 0;
				for(int i = 0; i < expIntervals.Count; i++) 
				{
					needExp += expIntervals[i];
					_expAllIntervals.Add(needExp);
                }
            }

			return _expAllIntervals; 
		}
	}
}
