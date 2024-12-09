using Godot;
using System;

[GlobalClass]
public partial class LocalSettingResource : Resource, IClone<LocalSettingResource>
{
    [Export] public int bgmVolume = 50;
    [Export] public int soundVolume = 50;

    private double _gameSpeedSetting = 1;
    [Export] public double gameSpeedSetting 
    {
        get 
        {
            return _gameSpeedSetting;
        }
        set 
        {
            if(gameSpeedSetting != value) 
            {
                _gameSpeedSetting = value;
                onGameSpeedSettingChange?.Invoke(_gameSpeedSetting);
            }
        }
    }

    public event Action<double> onGameSpeedSettingChange;


    public LocalSettingResource Clone()
    {
        LocalSettingResource result = CreateInstanceForClone();
        result.bgmVolume = bgmVolume;
        result.soundVolume = soundVolume;
        result.gameSpeedSetting = gameSpeedSetting;
        return result;
    }

    public LocalSettingResource CreateInstanceForClone()
    {
        return new LocalSettingResource();
    }
}
