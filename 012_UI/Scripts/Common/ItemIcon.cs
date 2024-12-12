using Godot;
using System;

public partial class ItemIcon : Control
{
	[Export] private TextureRect itemImage;

	private ItemBaseResource _data;

	public ItemBaseResource data 
	{
		get { return _data; }
		private set { _data = value; }
	}

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

	}
}
