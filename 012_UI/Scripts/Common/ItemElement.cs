using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class ItemElement : Control
{
	public enum SelectedState
	{
		None = 0,
		Hover = 1,
		Selected = 2,
	}


	[Export] private int index = -1;

	private bool _showProcess = true;
    [Export] public bool showProcess 
	{
		get { return _showProcess; }
		set { _showProcess = value; }
	}
	[Export] private AnimationPlayer animation;
    [Export] private ItemElementAreaPool areaPool;
    [Export] private TextureRect image;
	[Export] private TextureRect productImage;
	[Export] private Label durabilityLabel;
	[Export] private Control breakHintNode;
	[Export] private NinePatchRect selectedHint;
	[Export] public Button mainButton;
	[Export] public TextureButton dropButton;
    [Export] private TextureRect circleProgressImage;
    [Export] private Shader clockMaskShader;
	[Export] private Color normalColor;
    [Export] private Color pauseColor;
	[Export] private Color showColor;
	[Export] private Color hideColor;

    private ItemBaseResource _item;
	public ItemBaseResource item
	{
		get { return _item; }
		private set { _item = value; }
	}

	private List<AreaResource> areas = new List<AreaResource>();

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

	private bool _isPicking = false;
	public bool isPicking
    {
		get { return _isPicking; }
		set 
		{
            if (_isPicking != value) 
			{
                _isPicking = value;
                OnViewStateChange();
            }
		}
	}

	private bool _isFlying = false;
    public bool isFlying
    {
        get { return _isFlying; }
        set 
		{
			if(_isFlying != value) 
			{
				_isFlying = value;
				OnViewStateChange();
            }
		}
    }

	private void OnViewStateChange() 
	{
		if(isPicking || isFlying) 
		{
            image.Texture = null;
            productImage.Visible = false;
            durabilityLabel.Visible = false;
            circleProgressImage.Visible = false;
        }
		else 
		{
			SetView();
        }
	}

    public event Action<int> onMainButtonDown;
    public event Action<int> onMainButtonUp;
	public event Action<int> onMainRightPressed;
    public event Action<int> onMouseEnter;
    public event Action<int> onMouseExit;
	public event Action<int> onDropPressed;

    public const string clip_idle = "idle";
    public const string clip_shine = "shine";

    public const string material_selfColor = "selfColor";
    public const string material_maskColor = "maskColor";
	public const string material_percent = "percent";
	public const string material_atlasSize = "atlasSize";

    public override void _Ready()
    {
        base._Ready();

        InitMaterial();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        SetNowPercent(delta);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

		UnregisterEvent(item);
    }

    public void SetData(int index,ItemBaseResource item)
    {
		this.index = index;
		SetData(item);
    }

    public void SetData(ItemBaseResource item) 
	{
		UnregisterEvent(this.item);
        this.item = item;
		RegisterEvent(this.item);
        //InitMaterial();
        SetView();
    }

    public void SetArea(AreaIndex areaIndex)
    {
		//Debug.Print($"index:{index}, SetArea areaIndex:{areaIndex}");

		areas.Clear();

        for (int i = 0; i < 31; i++) 
		{
			int bit = 0;
			if(i > 0) 
			{
				bit = 1 << (i - 1);
			}

			if(bit == (int)AreaIndex.Normal) 
			{
				if((bit & (int)areaIndex) == bit) 
				{
					Modulate = showColor;
                }
				else 
				{
                    Modulate = hideColor;
                }
			}



            if ((bit & (int)areaIndex) == bit) 
			{
	            //Debug.Print($"bit:{bit}, areaIndex:{areaIndex}, (bit & (int)areaIndex) == 1 << i:{true}");
				if(GameManager.instance.areaConfig.config.TryGetValue((AreaIndex)bit, out AreaResource area)) 
				{
					areas.Add(area);
					SetAreaView();
				}
				else 
				{
					Debug.PrintErr($"未定義該AreaIndex:{areaIndex}");
				}
			}
		}
    }

    private void SetView() 
	{
        SetImage();
		SetProductImage();
        SetDurabilityLabel();
        SetCircleColor();
        SetCircleProgress();
		SetNowPercent(0);

    }

	private void SetAreaView() 
	{
		areaPool.ReturnAllElement();
        for (int i = 0; i < areas.Count; i++) 
		{
			if (areas[i].index != AreaIndex.None) 
			{
				ItemElementArea areaElemment = areaPool.GetElement();
				areaElemment.SetData(areas[i].index);
			}
        }
	}

	private void RegisterEvent(ItemBaseResource item) 
	{
		if(item is IProduce produce) 
		{
			produce.onIsProducingChange += OnProducingChange;
            produce.onDurabilityChange += OnDurabilityChange;
        }
        else if (item is IUseable useable)
        {
            useable.onIsUsingChange += OnUsingChange;
            useable.onDurabilityChange += OnDurabilityChange;
			useable.onUseUp += OnUseUp;
        }
    }

	private void UnregisterEvent(ItemBaseResource item) 
	{
        if (item is IProduce produce)
        {
            produce.onIsProducingChange -= OnProducingChange;
            produce.onDurabilityChange -= OnDurabilityChange;
        }
        else if (item is IUseable useable) 
		{
            useable.onIsUsingChange -= OnUsingChange;
            useable.onDurabilityChange -= OnDurabilityChange;
            useable.onUseUp -= OnUseUp;
        }
	}


    private void InitMaterial()
	{
		if(circleProgressImage.Material == null) 
		{
			ShaderMaterial material = new ShaderMaterial();
			material.Shader = clockMaskShader;
			material.SetShaderParameter(material_selfColor, new Vector4(normalColor.R, normalColor.G, normalColor.B, normalColor.A));
			material.SetShaderParameter(material_maskColor, new Vector4(1f, 1f, 1f, 0.0f));
			material.SetShaderParameter(material_percent, 1f);
			material.SetShaderParameter(material_atlasSize, new Vector2(1f, 1f));
			circleProgressImage.Material = material;
		}
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

	private void SetCircleColor() 
	{
        if (circleProgressImage.Material is ShaderMaterial material) 
		{
			if (item is IProduce produce)
			{
				if (produce.isProducing)
				{
                    material.SetShaderParameter(material_selfColor, new Vector4(normalColor.R, normalColor.G, normalColor.B, normalColor.A));
                }
				else 
				{
                    material.SetShaderParameter(material_selfColor, new Vector4(pauseColor.R, pauseColor.G, pauseColor.B, pauseColor.A));
                }
			}
			else if (item is IUseable useable)
			{
				if (useable.isUsing)
				{
                    material.SetShaderParameter(material_selfColor, new Vector4(normalColor.R, normalColor.G, normalColor.B, normalColor.A));
                }
				else
				{
                    material.SetShaderParameter(material_selfColor, new Vector4(pauseColor.R, pauseColor.G, pauseColor.B, pauseColor.A));
                }
			}
		}
    }

	private void SetNowPercent(double deltaTime) 
	{
        if (item is IProduce produce) 
		{
            if (circleProgressImage.Material is ShaderMaterial material)
            {
                float nowPercent = (float)(produce.nowTime / produce.needTime);
                material.SetShaderParameter(material_percent, nowPercent);
            }
        }
		else if(item is IUseable useable) 
		{
            if (circleProgressImage.Material is ShaderMaterial material)
            {
                float nowPercent = (float)(useable.nowTime / useable.needTime);
                material.SetShaderParameter(material_percent, nowPercent);
            }
        }
    }

	private void SetDurabilityLabel() 
	{
		if(isPicking || isFlying) 
		{
            durabilityLabel.Visible = false;
			return;
        }

		if(item is IUseable useable) 
		{
			if(item is IAttack attack) 
			{
                if (useable.durability > 0)
                {
                    breakHintNode.Visible = false;
                    durabilityLabel.Visible = true;
                    durabilityLabel.Text = $"{useable.durability}";
                }
                else
                {
                    durabilityLabel.Visible = false;
					breakHintNode.Visible = true;
                }
            }
			else 
			{
				if(useable.durability > 0) 
				{
					durabilityLabel.Visible = true;
					durabilityLabel.Text = $"{useable.durability}";
				}
				else 
				{
					durabilityLabel.Visible = false;
				}
			}
		}
		else if (item is IProduce produce && !(item is SelfToolResource)) 
		{
			if(produce.durability > 0) 
			{
				durabilityLabel.Visible = true;
				durabilityLabel.Text = $"{produce.durability}";
			}
			else 
			{
                durabilityLabel.Visible = false;
            }
        }
		else 
		{
			durabilityLabel.Visible = false;
        }
	}

	private void SetCircleProgress() 
	{
		if (!showProcess) 
		{
            circleProgressImage.Visible = false;
            return;
		}

		if(item is IProduce produce || item is IUseable useable) 
		{
			circleProgressImage.Visible = true;
		}
		else 
		{
            circleProgressImage.Visible = false;
        }
	}

	public void SetDropButton(bool isShow) 
	{
		dropButton.Visible = isShow;
    }

	private void OnDurabilityChange(int durability) 
	{
		SetDurabilityLabel();
    }

    private void OnProducingChange(bool isProducing)
    {
        SetCircleColor();
    }

    private void OnUsingChange(bool isUsing) 
	{
		SetCircleColor();
	}

	private void OnUseUp() 
	{

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

	private void OnMainButtonDown(bool isRight) 
	{
		//Debug.Print($"OnMainButtonDown:{index}");
		if (!isRight) 
		{
			onMainButtonDown?.Invoke(index);
		}
	}

	private void OnMainButtonUp(bool isRight) 
	{
		//Debug.Print($"OnMainButtonUp:{index}");
		if (!isRight) 
		{
	        onMainButtonUp?.Invoke(index);
		}
    }

	private void OnMainPressed(bool isRight) 
	{
        if (isRight)
        {
            onMainRightPressed?.Invoke(index);
        }
    }

	private void OnDropButtonPressed() 
	{
		onDropPressed?.Invoke(index);
    }
}
