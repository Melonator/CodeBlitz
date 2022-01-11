using Godot;
using System;

public class TextBox : Control
{
    [Signal] public delegate void Enter(string token1, string token2, string token3);
    private Label _textBox;
    public override void _Ready()
    {
        _textBox = GetNode<Label>("TextBox");
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey eventKey && @event.IsPressed()) 
        {
            if(eventKey.IsActionPressed("ui_accept"))
            {
                string[] tokens = _textBox.Text.Split(' ');
                if(tokens.Length == 2)
                    EmitSignal(nameof(Enter), tokens[0], tokens[1]);
                else if (tokens.Length == 3)
                    EmitSignal(nameof(Enter), tokens[0], tokens[1], tokens[2]);

                _textBox.Text = string.Empty;
            }
            else if (eventKey.IsActionPressed("backspace"))
            {
                if(_textBox.Text.Length > 0)
                    _textBox.Text = _textBox.Text.Remove(_textBox.Text.Length - 1);
            }
            else 
            {
                  _textBox.Text += (char)eventKey.Scancode;
                   _textBox.Text = _textBox.Text.ToLower();
            }
              
   
        }
    }
}

