using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleHPInfoElement : Control
{
	[Export] Label hpLabel;
	[Export] AnimationPlayer animation;

	public const string clip_show = "show";
	public const string string_NoDamage = "ts_common_NoDamage";


    public async Task ShowMinusHP(int hp, Vector2 globalPosition, HPChangeType type) 
	{
		switch (type) 
		{
			case HPChangeType.None:
			case HPChangeType.Normal:
				if(hp > 0) 
				{
					hpLabel.Text = $"-{hp}";
				}
				else 
				{
                    hpLabel.Text = $"{Tr(string_NoDamage)}";
                }
                break;
			case HPChangeType.Crash:
                if (hp > 0) 
				{
                    hpLabel.Text = $"-{hp} {Tr($"{LanguagePrefix.hpChangeType}{type}")}!";
				}
		        else
                {
                    hpLabel.Text = $"{Tr(string_NoDamage)}{Tr($"{LanguagePrefix.hpChangeType}{type}")}";
                }
                break;
		}
		GlobalPosition = globalPosition;

        animation.Play(clip_show);
        await ToSignal(animation, "animation_finished").ToTask();
    }
}
