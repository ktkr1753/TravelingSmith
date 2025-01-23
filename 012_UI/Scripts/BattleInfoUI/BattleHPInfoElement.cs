using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleHPInfoElement : Control
{
	[Export] Label hpLabel;
	[Export] AnimationPlayer animation;

	public const string clip_show = "show";

	public async Task ShowMinusHP(int hp, Vector2 globalPosition, HPChangeType type) 
	{
		switch (type) 
		{
			case HPChangeType.None:
			case HPChangeType.Normal:
                hpLabel.Text = $"-{hp}";
                break;
			case HPChangeType.Crash:
                hpLabel.Text = $"-{hp} {Tr($"{LanguagePrefix.hpChangeType}{type}")}!";
                break;
		}
		GlobalPosition = globalPosition;

        animation.Play(clip_show);
        await ToSignal(animation, "animation_finished").ToTask();
    }
}
