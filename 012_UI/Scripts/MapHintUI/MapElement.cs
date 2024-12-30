using Godot;
using System;

public partial class MapElement : Control
{
	public enum ShowType 
	{
		None = 0,
		Monster = 1,
		Shop = 2,
	}


	[Export] private TextureRect image;
	[Export] private ProgressBar hpProgressBar;
	[Export] private Label distanceLabel;

	private int _showDis = 0;
	public int showDis 
	{
		get 
		{
			if(_showDis == 0) 
			{
				_showDis = (int)(GetViewportRect().Size.X / 2) + 12;
            }

			return _showDis;
		}
	}

    private ShowType type = ShowType.None;
	private MonsterObject monsterObjectData;

	public event Action<MapElement> onDestroy;

	public double nowTime = 0;
	public const double showFreq = 0.5;

    public override void _Process(double delta)
    {
        base._Process(delta);

		nowTime += delta;

		if(nowTime >= showFreq)
		{
			nowTime -= showFreq;
            switch (type) 
			{
				case ShowType.Monster:
					{
						SetMonsterDistance();
					}
					break;
			}
		}
    }



    public void SetMonsterData(MonsterObject monsterObject) 
	{
        UnregisterMonsterEvent();
        monsterObjectData = monsterObject;
        type = ShowType.Monster;
		RegisterMonsterEvent();
		SetMonsterView();
    }

	public void SetShopData() 
	{
        type = ShowType.Shop;
    }

	public void SetNullData() 
	{
        UnregisterMonsterEvent();
        monsterObjectData = null;
        type = ShowType.None;
		nowTime = 0;
    }

	private void SetMonsterView() 
	{
		SetMonsterImage();
        SetMonsterDistance();
        InitMonsterHPBar();
    }

	private void SetMonsterImage() 
	{
		image.Texture = monsterObjectData.mainImage.Texture;
	}

	private void InitMonsterHPBar() 
	{
		hpProgressBar.MaxValue = monsterObjectData.data.maxHp;
		hpProgressBar.Value = monsterObjectData.data.nowHp;
	}

	private void SetMonsterDistance() 
	{
		float distance = Mathf.Abs(monsterObjectData.GlobalPosition.DistanceTo(GameManager.instance.mapManager.nowMap.targetPoint.GlobalPosition));
		int showDistance = (int)distance;

		if(showDistance <= showDis) 
		{
            onDestroy?.Invoke(this);
        }
		else 
		{
			distanceLabel.Text = $"{showDistance}m";
		}
    }


	private void RegisterMonsterEvent() 
	{
		monsterObjectData.data.onHPChange += OnHPChange;
		monsterObjectData.onDestroy += OnMonsterDestroy;
        monsterObjectData.onDie += OnMonsterDie;
    }

    private void UnregisterMonsterEvent()
    {
		if(monsterObjectData != null) 
		{
			monsterObjectData.data.onHPChange -= OnHPChange;
			monsterObjectData.onDestroy -= OnMonsterDestroy;
            monsterObjectData.onDie -= OnMonsterDie;
        }
    }

	private void OnHPChange(int preHP,int nowHP) 
	{
        hpProgressBar.Value = nowHP;
    }

	private void OnMonsterDestroy(MonsterObject monsterObject) 
	{
		onDestroy?.Invoke(this);
    }

	private void OnMonsterDie(MonsterObject monsterObject) 
	{
        onDestroy?.Invoke(this);
    }
}
