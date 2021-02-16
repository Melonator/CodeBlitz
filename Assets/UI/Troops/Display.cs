using Godot;

namespace CodeBlitz.Assets.UI.Troops
{
    public class Display : Node
    {
        [Export] public string Description;
        [Export] public int Damage;
        [Export] public int Health;
    }
}
