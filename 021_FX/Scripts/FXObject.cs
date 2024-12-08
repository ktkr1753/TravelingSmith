using Godot;
using System;
using System.Threading.Tasks;

public partial class FXObject : Node2D
{
	[Export] Sprite2D image;
    [Export] AnimationPlayer animation;

    public const string clip_FX1 = "FX1";

	public async Task PlayFX(FXEnum fxEnum) 
	{
		bool isFail = false;
		switch (fxEnum) 
		{
			case FXEnum.FX1:
				{
					animation.Play(clip_FX1);
                }
				break;
			default: 
				{
					Debug.PrintErr($"未定義fx:{fxEnum}");
					isFail = true;
				}
				break;
		}

		if (!isFail) 
		{
			await ToSignal(animation, "animation_finished").ToTask();
		}
		//QueueFree();
    }
}
