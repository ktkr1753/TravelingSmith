using Godot;
using System;

public partial class PickFeatureElement : Control
{
	[Export] private Label nameLabel;
	[Export] private TextureRect image;
	[Export] private NinePatchRect selectedHint;
	[Export] private Label descriptionLabel;

	private int index = -1;

	private FeatureResource feature = null;

	public event Action<int> onMainClick;

	public void SetData(int index, FeatureResource feature) 
	{
		this.index = index;
		this.feature = feature;

		if(index == -1 || feature == null) 
		{
			return;
		}

		SetView();
	}

	private void SetView() 
	{
		SetName();
		SetDescription();
    }

	private void SetName() 
	{
		nameLabel.Text = Tr($"{LanguagePrefix.featureName}{feature.index}");
	}

    private void SetDescription()
    {
        descriptionLabel.Text = Tr($"{LanguagePrefix.featureDescription}{feature.index}");
    }

	public void SetSelectedHint(bool isOn) 
	{
		selectedHint.Visible = isOn;
    }

	public void OnMainClick() 
	{
		onMainClick?.Invoke(index);

    }
}
