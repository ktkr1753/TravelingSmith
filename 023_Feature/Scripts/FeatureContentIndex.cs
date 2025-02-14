using Godot;
using System;

public enum FeatureContentIndex
{
	None = 0,
	//Good
	HighSpeedProduce = 1,	//加快物品製作速度
	HighSell = 2,           //增加物品販賣價格
	Shell = 3,				//免除一次攻擊的護盾(使用後有冷卻)
	AddMainHp = 4,			//增加血量上限
	AddCrashDamage = 5,		//增加撞擊傷害
	AddAttackDamage = 6,	//增加物品(無磨損時)攻擊傷害

    //Bad
    HighMonsterHp = 101,		//增加怪物最大血量
	LowMainHp = 102,			//減少最大血量
	LoseMoney = 103,			//每次過商店會減少金錢
	NoCrashDamage = 104,		//不再有撞擊傷害
    AddDurabilityCost = 105,	//增加物品耐久度消耗
	LowSellMoney = 106,			//減少販賣的金錢
}
