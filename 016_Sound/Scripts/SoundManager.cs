using Godot;
using System;
using System.Threading.Tasks;

public partial class SoundManager : Node
{
    public enum PlayAgainStrategy
    {
        PlayOneNew = 0,         //再撥一個新的
        PlayFromBegan = 1,      //從頭播放
        StillPlaying = 2,       //繼續撥舊的
    }

    [Export] public AudioStreamPlayer bgmPlayer;
    [Export] public Godot.Collections.Array<AudioStreamPlayer> soundPlayers = new Godot.Collections.Array<AudioStreamPlayer>();


    public const string soundPreFix = "res://016_Sound/Sounds/";
    public const int maxVolume = 100;
    public const int miniVolume = 0;

    public const double fadeDuration = 1;

    private int _bgmVolume = 0;
    public int bgmVolume
    {
        get { return _bgmVolume; }
        set
        {
            if (_bgmVolume != value)
            {
                _bgmVolume = value;
                OnBGMVolumeChange(_bgmVolume);
            }
        }
    }
    private int _soundVolume = 0;
    public int soundVolume
    {
        get { return _soundVolume; }
        set
        {
            if (_soundVolume != value)
            {
                _soundVolume = value;
                OnSoundVolumeChange(_soundVolume);
            }
        }
    }

    public void Init()
    {
        bgmVolume = GameManager.instance.localSetting.bgmVolume;
        soundVolume = GameManager.instance.localSetting.soundVolume;
    }

    public async void PlayBGM(string bgmName)
    {
        AudioStream stream = GD.Load<AudioStream>($"{soundPreFix}{bgmName}");
        
        if(bgmPlayer.Stream != null && bgmPlayer.Playing) 
        {
            await FadeOutBGM();
        }

        if (stream != null)
        {
            bgmPlayer.Stream = stream;
            bgmPlayer.Play();
            await FadeInBGM();
        }
    }


    public void PlaySound(string soundName, PlayAgainStrategy playAgainStrategy = PlayAgainStrategy.PlayOneNew)
    {
        //Debug.Print($"PlaySound soundName:{soundName}, playAgainStrategy:{playAgainStrategy}");
        int findPlayingSameIndex = -1;
        string fileName = $"{soundName}";
        for (int i = 0; i < soundPlayers.Count; i++)
        {
            if (soundPlayers[i].Stream != null && soundPlayers[i].Playing && soundPlayers[i].Stream.ResourcePath.GetFile() == fileName)
            {
                findPlayingSameIndex = i;
                break;
            }
        }
        //Debug.Print($"findPlayingSameIndex:{findPlayingSameIndex}");

        if (findPlayingSameIndex != -1)
        {
            switch (playAgainStrategy)
            {
                case PlayAgainStrategy.PlayOneNew:
                    {
                        AudioStream stream = GD.Load<AudioStream>($"{soundPreFix}{fileName}");
                        if (stream != null)
                        {
                            AudioStreamPlayer soundPlayer = GetIdleSoundAudioStream();
                            soundPlayer.Stream = stream;
                            soundPlayer.Play();
                        }
                    }
                    break;
                case PlayAgainStrategy.StillPlaying:
                    {
                        //不做事
                    }
                    break;
                case PlayAgainStrategy.PlayFromBegan:
                    {
                        soundPlayers[findPlayingSameIndex].Stop();
                        soundPlayers[findPlayingSameIndex].Play(0);
                    }
                    break;
            }
        }
        else
        {
            AudioStream stream = GD.Load<AudioStream>($"{soundPreFix}{fileName}");
            if (stream != null)
            {
                AudioStreamPlayer soundPlayer = GetIdleSoundAudioStream();
                soundPlayer.Stream = stream;
                soundPlayer.Play();
            }
        }
    }

    public async void StopBGM() 
    {
        if (bgmPlayer.Stream != null && bgmPlayer.Playing)
        {
            await FadeOutBGM();
            bgmPlayer.Stop();
        }
    }

    public void StopSound(string soundName, bool stopAll = true)
    {
        string fileName = $"{soundName}";
        for (int i = 0; i < soundPlayers.Count; i++)
        {
            if (soundPlayers[i].Stream != null && soundPlayers[i].Playing && soundPlayers[i].Stream.ResourcePath.GetFile() == fileName)
            {
                //Debug.Print($"停止第{i}個");
                soundPlayers[i].Stop();
                if (!stopAll)
                {
                    break;
                }
            }
        }
    }

    private void OnBGMVolumeChange(int bgmVolume)
    {
        bgmPlayer.VolumeDb = VolumeToDb(bgmVolume);
        if (GameManager.instance.localSetting.bgmVolume != bgmVolume)
        {
            GameManager.instance.localSetting.bgmVolume = bgmVolume;
        }
        //Debug.Print($"bgmVolume:{bgmVolume},bgmPlayer.VolumeDb:{bgmPlayer.VolumeDb}");
    }

    private void OnSoundVolumeChange(int soundVolume)
    {
        for (int i = 0; i < soundPlayers.Count; i++)
        {
            soundPlayers[i].VolumeDb = VolumeToDb(soundVolume);
        }
        if (GameManager.instance.localSetting.soundVolume != soundVolume)
        {
            GameManager.instance.localSetting.soundVolume = soundVolume;
        }
        //Debug.Print($"bgmVolume:{bgmVolume},soundPlayer.VolumeDb:{soundPlayer.VolumeDb}");
    }

    private AudioStreamPlayer GetIdleSoundAudioStream()
    {
        AudioStreamPlayer result = soundPlayers[0];
        for (int i = 0; i < soundPlayers.Count; i++)
        {
            if (soundPlayers[i].Playing == false)
            {
                //Debug.PrintWarn($"GetIdleSoundAudioStream 取得第{i}個");
                result = soundPlayers[i];
                break;
            }
        }
        return result;
    }

    private async Task FadeInBGM() 
    {
        await FadeInInternal(bgmPlayer, bgmVolume);
    }

    private async Task FadeOutBGM()
    {
        await FadeOutInternal(bgmPlayer, bgmVolume);
    }

    private async Task FadeInInternal(AudioStreamPlayer player, int finalValue) 
    {
        player.VolumeDb = VolumeToDb(miniVolume);
        float finalDb = VolumeToDb(finalValue);
        Tween tween = CreateTween();
        tween.SetEase(Tween.EaseType.In);
        tween.TweenProperty(player, "volume_db", finalDb, fadeDuration);
        await ToSignal(tween, "finished");
    }

    private async Task FadeOutInternal(AudioStreamPlayer player, int startValue)
    {
        player.VolumeDb = VolumeToDb(startValue);
        float finalDb = VolumeToDb(miniVolume);
        Tween tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(player, "volume_db", finalDb, fadeDuration);
        await ToSignal(tween, "finished");
    }

    private float VolumeToDb(int volume) 
    {
        float result = Math.Clamp(Mathf.LinearToDb(((float)volume / maxVolume)) + 5, -30, 30);
        return result;
    }
}
