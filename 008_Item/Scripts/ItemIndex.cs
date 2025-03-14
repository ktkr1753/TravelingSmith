using Godot;
using System;

public enum ItemIndex
{
	None = 0,
    //武器 1~100
    Dart = 1,
    StraightSword = 2,
    HuntingBow = 3,
    KnightsSword = 4,
    MatchStaff = 5,
    CeremonialStaff = 6,
    StoneKnife = 7,
    //回復 101~200
    Board = 101,
    //配方 201~300
    RecipeDart = 201,
    RecipeStraightSword = 202,
    RecipeBoard = 203,
    RecipeHuntingBow = 204,
    TitaniumForge = 205,                        //之後刪掉
    RecipeKnightsSword = 206,
    RecipePearlNecklace = 207,
    RecipeIronRing = 208,
    GoldForge = 209,                            //之後刪掉
    RecipeMatchStaff = 210,
    RecipeCeremonialStaff = 211,
    FlareGemstoneBurin = 212,
    RecipeWoodenWheel = 213,
    RecipeIronWheel = 214,
    IronForge = 215,                            //之後刪掉
    RecipePickaxe = 216,
    RecipeFellingAxe = 217,
    RecipeFlareGemstonePickaxe = 218,
    RecipeIronForge = 219,
    RecipeGoldForge = 220,
    RecipeTitaniumForge = 221,
    RecipeFlareGemstoneBurin = 222,
    RecipeWoodenChest = 223,
    RecipeTorch = 224,
    RecipeIronChest = 225,
    RecipeWorkbench = 226,
    RecipeStoneGolem = 227,
    RecipeButcherKnife = 228,
    RecipeStoneKnife = 229,
    RecipeIron = 230,                           //之後刪掉
    RecipeGold = 231,                           //之後刪掉
    RecipeFlareGemstone = 232,  
    RecipeTitanium = 233,                       //之後刪掉
    Forge = 234,
    RecipeForge = 235,
    //生產 301~400
    Pickaxe = 301,
    FellingAxe = 302,
    FlareGemstonePickaxe = 303,
    StonePickaxe = 304,
    ButcherKnife = 305,
    //引擎 401~500
    WoodenWheel = 401,
    IronWheel = 402,
    //換金道具 501~600
    PearlNecklace = 501,
    IronRing = 502,
    //素材 601~700
    Iron = 601,
	Gold = 602,
    Branch = 603,
    Titanium = 604,
    Pearl = 605,
    GoldOre = 606,
    FlareGemstoneOre = 607,
    FlareGemstone = 608,
    Stone = 609,
    IronOre = 610,
    Paper = 611,
    Parchment = 612,
    Fur = 613,
    //場域 701~800
    WoodenChest = 701,
    Torch = 702,
    IronChest = 703,
    Workbench = 704,
    StoneGolem = 705,
}
