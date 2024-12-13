using Godot;
using System;

//Button的自製延伸版本，新增了區分左右鍵按下的功能

[GlobalClass]
public partial class MultiButton : Button
{
	private bool _right = false;

	[Signal] public delegate void button_downExEventHandler(bool isRight);
    [Signal] public delegate void button_upExEventHandler(bool isRight);
    [Signal] public delegate void pressedExEventHandler(bool isRight);

    public override void _Ready()
    {
        base._Ready();
        ButtonDown += OnButtonDown;
        ButtonUp += OnButtonUp;
        Pressed += OnPressed;
    }
    public override void _ExitTree()
    {
        base._ExitTree();
    }


    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);

        if(@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed) 
        {
            _right = mouseButtonEvent.ButtonIndex == MouseButton.Right;
        }
    }

    private void OnButtonDown()
    {
        EmitSignal(SignalName.button_downEx, _right);
    }

    private void OnButtonUp() 
    {
        EmitSignal(SignalName.button_upEx, _right);
    }

    private void OnPressed() 
    {
        EmitSignal(SignalName.pressedEx, _right);
    }

    /*
    public override void _Pressed()
    {
        base._Pressed();

        if (_right) 
        {
            GrabFocus();
        }
        EmitSignal(SignalName.pressedEx, _right);
    }
    */
}
