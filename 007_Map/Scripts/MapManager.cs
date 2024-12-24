using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MapManager : Node
{
    [Export] public Map nowMap;
    [Export] public FXObjectPool fxPool;

    public void Init() 
    {
        nowMap.Init();
    }

    public async Task PlayFX(FXEnum fxEnum, Vector2 position) 
    {
        FXObject fx = fxPool.GetElement();
        fx.Position = position;
        await fx.PlayFX(fxEnum);
        fxPool.ReturnElement(fx);
    }


    public List<KeyValuePair<double, MonsterObject>> FindMonsterInRange(double range) 
    {
        List<KeyValuePair<double, MonsterObject>> result = new List<KeyValuePair<double, MonsterObject>>();

        Vector2 targetPos = nowMap.targetPoint.GlobalPosition;
        for(int i = 0; i < nowMap.monsters.Count; i++) 
        {
            float distance = targetPos.DistanceTo(nowMap.monsters[i].GlobalPosition);
            if (distance <= range) 
            {
                result.Add(new KeyValuePair<double, MonsterObject>(distance, nowMap.monsters[i]));
            }
        }

        result.Sort((a, b) =>
        {
            return (int)((a.Key - b.Key) * 1000);
        });

        return result;
    }
}
