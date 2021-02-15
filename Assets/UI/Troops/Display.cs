using Godot;
using System;

namespace CodeCombat.Display
{
    public class Display : Node
    {
        [Export] public string Description;
        [Export] public int Damage;
        [Export] public int Health;
    }
}
