using Godot;
using System;

public partial class ItemIcon : Control
{
	[Export] private TextureRect itemImage;
    [Export] public Material outLineMaterial;
    private ItemBaseResource _data;

	public ItemBaseResource data 
	{
		get { return _data; }
		private set { _data = value; }
	}

	public event Action<ItemIcon> onMainClick;

	public void SetData(ItemBaseResource item) 
	{
		this.data = item;
		SetView();
    }

	private void SetView() 
	{
		SetImage();
	}

	private void SetImage() 
	{
		itemImage.Texture = data?.texture;
    }


	public void OnMainButtonClick()
	{
		onMainClick?.Invoke(this);
    }

    public void OnMouseEnter()
    {
        itemImage.Material = outLineMaterial;
    }

    public void OnMouseExit()
    {
        itemImage.Material = null;
    }
}
