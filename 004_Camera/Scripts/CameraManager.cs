using Godot;
using System;

public partial class CameraManager : Node2D
{
	[Export] private Node2D cameraControlNode;
    [Export] public Camera2DEX camera;
    [Export] float shakeFade = 10f;
    private float shakeStrength = 0;

    private float defaultShakeStrength = 20f;
    public const double smoothingSpeed = 3;

    public void Init() 
    {
        GameManager.instance.onNeedPauseChange += OnGamePauseChnage;
        GameManager.instance.localSetting.onGameSpeedSettingChange += OnGameSpeedChange;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        GameManager.instance.onNeedPauseChange -= OnGamePauseChnage;
        GameManager.instance.localSetting.onGameSpeedSettingChange -= OnGameSpeedChange;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        UpdateShakeCamera(delta);
    }

    public void ShakeCamera(float shakeStrength = 0) 
    {
        if(shakeStrength == 0) {
            this.shakeStrength = defaultShakeStrength;    
        }
        else 
        {
            this.shakeStrength = shakeStrength;
        }
    }

    private void UpdateShakeCamera(double delta) 
    {
        if (shakeStrength > 0)
        {
            shakeStrength = Mathf.Lerp(shakeStrength, 0, shakeFade * (float)delta);
        }

        Vector2 randomOffset = new Vector2(GameManager.instance.randomManager.GetRange(RandomType.Other, -shakeStrength, shakeStrength),
            GameManager.instance.randomManager.GetRange(RandomType.Other, -shakeStrength, shakeStrength));

        camera.Offset = randomOffset;
    }

    public void SetCameraPositionSmoothing() 
    {
        camera.PositionSmoothingSpeed = (float)(smoothingSpeed * GameManager.instance.gameSpeed);
    }

    public void SetLimit(Rect2I limitRect) 
    {
        camera.LimitLeft = limitRect.Position.X;
        camera.LimitRight = limitRect.Size.X;
        camera.LimitTop = limitRect.Position.Y;
        camera.LimitBottom = limitRect.Size.Y;
    }

    private void OnGamePauseChnage(int needPauseNum) 
    {
        SetCameraPositionSmoothing();
    }
    private void OnGameSpeedChange(double gameSpeed) 
    {
        SetCameraPositionSmoothing();
    }
}
