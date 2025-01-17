using Godot;
using System;

public partial class ShopObject : Node2D
{
    public enum CharacterState 
    {
        None = 0,
        Out = 1,
        In = 2,
    }

    [Export] private MapCharacter character;
    [Export] public Node2D enterShopNode;
    [Export] public Node2D exitShopNode;

    private CharacterState _nowCharacterState = CharacterState.Out;

    public CharacterState nowCharacterState 
    {
        get { return _nowCharacterState; }
        set 
        { 
            if(_nowCharacterState != value) 
            {
                CharacterState preState = _nowCharacterState;
                _nowCharacterState = value;
                OnCharacterStateChange(preState, _nowCharacterState);
            }
        }
    }

    private void OnCharacterStateChange(CharacterState preState, CharacterState nowState) 
    {
        switch (nowState) 
        {
            case CharacterState.Out:
                PlayCharacterOut();
                break;
            case CharacterState.In:
                PlayCharacterIn();
                break;
        }
    }



    private int _index;
    public int index 
    {
        get 
        {
            return _index;
        }
        private set 
        {
            _index = value;
        }
    }

    public const float characterStartY = -80.0f;
    public const float characterEndY = -96.0f;

    public void SetIndex(int index) 
    {
        this.index = index;
    }

    public override void _Ready()
    {
        base._Ready();

        character.Init(new Vector2(GlobalPosition.X, GlobalPosition.Y + characterStartY));
    }

    public void PlayCharacterIn() 
    {
        character.PlayIn(new Vector2(GlobalPosition.X, GlobalPosition.Y + characterStartY), GlobalPosition);
    }

    public void PlayCharacterOut()
    {
        character.PlayOut(GlobalPosition, new Vector2(GlobalPosition.X, GlobalPosition.Y + characterEndY));
    }
}
