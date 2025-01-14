using Godot;
using System;

public partial class ItemElementArea : Control
{
	[Export] TextureRect image;

	private AreaIndex areaIndex = AreaIndex.None;

    public void SetData(AreaIndex areaIndex) 
	{
		this.areaIndex = areaIndex;

        SetView();
    }

	private void SetView() 
	{
        if (GameManager.instance.areaConfig.config.TryGetValue(areaIndex, out AreaResource area)) 
		{
            image.Texture = area.texture;
		}
	}
}
