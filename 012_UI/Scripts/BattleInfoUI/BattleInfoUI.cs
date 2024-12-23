using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleInfoUI : UIBase
{
	[Export] public BattleHPInfoElementPool hpInfoPool;

	public async Task ShowMinusHPInfo(int hp, Vector2 globalPosition) 
	{
        BattleHPInfoElement element = hpInfoPool.GetElement();
        await element.ShowMinusHP(hp, globalPosition);

        hpInfoPool.ReturnElement(element);
    }
}
