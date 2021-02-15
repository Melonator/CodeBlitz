using Godot;
using System.Threading.Tasks;
using System;

public class MainMenu : Control
{
    private int _index = 1;
    private bool _canPress = false;
    public override void _Ready()
    {
        GetNode<Timer>("MenuTimer").Start();
    }

    public override void _Input(InputEvent @event)
    {
        if(_canPress)
        {
            if (@event.IsActionPressed("enter"))
            {
                GetNode<Timer>("StartTimer").Stop();
                _canPress = false;
                GetNode<Timer>("StartTimer2").Stop();
                GetNode<AnimationPlayer>("AnimationPlayer").Play("Fade Out");
            }
        }
      
    }
    private async void _on_Timer_timeout()
    {
        GetNode<Timer>("MenuTimer").Stop();
        GetNode<AudioStreamPlayer>("ExplosionPlayer").Play();
        GetNode<Label>($"{_index}").Show();
        _index++;
        if(_index == 3)
        {
            await Task.Delay(800);
            _canPress = true;
            GetNode<Timer>("StartTimer").Start();
            GetNode<Label>("3").Show();
            GetNode<AudioStreamPlayer>("StartPlayer").Play();
        }
        else
        {
            GetNode<Timer>("MenuTimer").Start();
        }
    }

    private void _on_AnimationPlayer_finished(string animName)
    {
        GetTree().ChangeScene("res://Levels/ClassSelect.tscn");
    }

    private async void _on_StartTimer_timeout()
    {
        GetNode<Timer>("StartTimer").Stop();
        GetNode<Label>("3").Hide();
        GetNode<Timer>("StartTimer2").Start();
        
    }

    private void _on_StartTimer2_timeout()
    {
        GetNode<Timer>("StartTimer2").Stop();
        GetNode<Label>("3").Show();
        GetNode<Timer>("StartTimer").Start();
    }
}
