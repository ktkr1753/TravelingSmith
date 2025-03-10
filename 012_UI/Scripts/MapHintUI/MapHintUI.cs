using Godot;
using System;

public partial class MapHintUI : UIBase
{
	[Export] private MapElementPool elementPool;

    public override void Init()
    {
        base.Init();

        GameManager.instance.mapManager.nowMap.onCreateMonster += OnCreateMonster;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.mapManager.nowMap.onCreateMonster -= OnCreateMonster;
    }



    private void OnCreateMonster(MonsterObject monsterObject) 
    {
        MapElement mapElement = elementPool.GetElement();
        mapElement.SetMonsterData(monsterObject);
        mapElement.onDestroy += OnElementDestroy;
    }

    private void OnElementDestroy(MapElement element) 
    {
        element.onDestroy -= OnElementDestroy;
        element.SetNullData();
        elementPool.ReturnElement(element);
    }
}
