using Godot;
using System;

[GlobalClass]
public partial class LocalSettingResource : Resource, IClone<LocalSettingResource>
{
    [Export] public int bgmVolume = 50;
    [Export] public int soundVolume = 50;

    [Export] public double gameSpeedSetting = 1;
    [Export] public bool isPause = false;


    public LocalSettingResource Clone()
    {
        LocalSettingResource result = CreateInstanceForClone();
        result.bgmVolume = bgmVolume;
        result.soundVolume = soundVolume;
        result.gameSpeedSetting = gameSpeedSetting;
        result.isPause = isPause;
        return result;
    }

    public LocalSettingResource CreateInstanceForClone()
    {
        return new LocalSettingResource();
    }
}
