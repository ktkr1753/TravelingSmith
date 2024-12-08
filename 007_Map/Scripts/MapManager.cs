using Godot;
using System;
using System.Threading.Tasks;

public partial class MapManager : Node
{
    [Export] public Map nowMap;
    [Export] public FXObjectPool fxPool;

    public async Task PlayFX(FXEnum fxEnum, Vector2 position) 
    {
        FXObject fx = fxPool.GetElement();
        fx.Position = position;
        await fx.PlayFX(fxEnum);
        fxPool.ReturnElement(fx);
    }
}
