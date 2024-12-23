using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleHPInfoElement : Control
{
	[Export] Label hpLabel;
	[Export] AnimationPlayer animation;

	public const string clip_show = "show";

	public async Task ShowMinusHP(int hp, Vector2 globalPosition) 
	{
        hpLabel.Text = $"-{hp}";
		GlobalPosition = globalPosition;


        animation.Play(clip_show);
        await ToSignal(animation, "animation_finished").ToTask();
    }
}
