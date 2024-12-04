using Godot;
using System;

[GlobalClass]
public partial class MainGameItemElement : Control
{
	public enum SelectedState
	{
		None = 0,
		Hover = 1,
		Selected = 2,
	}


	[Export] private int index;
	[Export] private AnimationPlayer animation;
	[Export] private TextureRect image;
	[Export] private TextureRect productImage;
	[Export] private Label durabilityLabel;
	[Export] private NinePatchRect selectedHint;

	private ItemBaseResource _item;
	public ItemBaseResource item
	{
		get { return _item; }
		private set { _item = value; }
	}

	private SelectedState _nowSelectedState = SelectedState.None;
	public SelectedState nowSelectedState 
	{
		get { return _nowSelectedState; }
		set 
		{
			if(_nowSelectedState != value)
			{
				SelectedState preState = _nowSelectedState;
				_nowSelectedState = value;
                OnSelectedStateChange(preState, _nowSelectedState);
            }
		}
	}

	private void OnSelectedStateChange(SelectedState preState, SelectedState nextState) 
	{
		switch (preState) 
		{
            case SelectedState.None:
                {

                }
                break;
            case SelectedState.Hover:
                {
                    selectedHint.Visible = false;
                }
                break;
            case SelectedState.Selected:
                {
                    selectedHint.Visible = false;
                }
                break;
			default:
				{
					Debug.PrintWarn($"未定義的狀態 preState:{preState}");
				}
				break;
        }
		switch (nextState) 
		{
			case SelectedState.None:
				{
                    animation.Stop();
                }
				break;
			case SelectedState.Hover:
				{
                    selectedHint.Visible = true;
                    animation.Play(clip_shine);
                }
				break;
			case SelectedState.Selected:
				{
					selectedHint.Visible = true;
                    animation.Play(clip_idle);
                }
				break;
            default:
                {
                    Debug.PrintWarn($"未定義的狀態 nextState:{nextState}");
                }
                break;
        }
	}


    public event Action<int> onMainButtonDown;
    public event Action<int> onMainButtonUp;
    public event Action<int> onMouseEnter;
    public event Action<int> onMouseExit;

    public const string clip_idle = "idle";
    public const string clip_shine = "shine";

	public void SetData(ItemBaseResource item) 
	{
		this.item = item;
		SetView();
    }

	private void SetView() 
	{
		SetImage();
		SetProductImage();
        SetDurabilityLabel();
	}

	private void SetImage() 
	{
		if(item == null) 
		{
			image.Texture = null;

        }
		else 
		{
	        image.Texture = item.texture;
		}
    }

	private void SetProductImage() 
	{
		if(item is IProduce produce) 
		{
			if(GameManager.instance.itemConfig.config.TryGetValue(produce.productItem, out ItemBaseResource productItem)) 
			{
                productImage.Visible = true;
                productImage.Texture = productItem.texture;
			}
			else 
			{
				Debug.PrintErr($"找不到該道具, index:{produce.productItem}");
			}
        }
		else 
		{
            productImage.Visible = false;
        }
	}

	private void SetDurabilityLabel() 
	{
		if(item is IUseable useable) 
		{
            durabilityLabel.Visible = true;
            durabilityLabel.Text = $"{useable.durability}";
		}
		else 
		{
			durabilityLabel.Visible = false;
        }
	}

	private void OnMouseEnter() 
	{
        //SetSelectedHint(true);
		onMouseEnter?.Invoke(index);
    }

	private void OnMouseExit() 
	{
		//SetSelectedHint(false);
        onMouseExit?.Invoke(index);
    }

	private void OnMainButtonDown() 
	{
		//Debug.Print($"OnMainButtonDown:{index}");
		onMainButtonDown?.Invoke(index);
	}

	private void OnMainButtonUp() 
	{
        //Debug.Print($"OnMainButtonUp:{index}");
        onMainButtonUp?.Invoke(index);
    }
}
