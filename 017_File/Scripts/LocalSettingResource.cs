using Godot;
using System;

[GlobalClass]
public partial class LocalSettingResource : Resource, IClone<LocalSettingResource>
{
    [Export] public int bgmVolume = 50;
    [Export] public int soundVolume = 50;

    public LocalSettingResource Clone()
    {
        LocalSettingResource result = CreateInstanceForClone();
        result.bgmVolume = bgmVolume;
        result.soundVolume = soundVolume;
        return result;
    }

    public LocalSettingResource CreateInstanceForClone()
    {
        return new LocalSettingResource();
    }
}
