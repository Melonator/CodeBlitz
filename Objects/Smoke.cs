using Godot;
using System;

namespace CodeCombat.Entity
{
    public class Smoke : Node2D
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            GetNode<AnimatedSprite>("AnimatedSprite").Play("default");  
        }

        private void _on_AnimatedSprite_finished()
        {
            QueueFree();
        }
    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
    }
}
