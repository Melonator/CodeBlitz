using CodeBlitz.Assets.UI.Troops;
using Godot;

namespace CodeBlitz.Levels.UI
{
    public class RedDisplay : Control
    {        
        private PackedScene _basicScene;
        private PackedScene _archerScene;
        private PackedScene _wizardScene;
        private PackedScene _tankScene;
        private PackedScene _knightScene;
        public override void _Ready()
        {
            _basicScene = ResourceLoader.Load("res://Assets/UI/Troops/Basic.tscn") as PackedScene;
            _archerScene = ResourceLoader.Load("res://Assets/UI/Troops/Archer.tscn") as PackedScene;
            _wizardScene = ResourceLoader.Load("res://Assets/UI/Troops/Wizard.tscn") as PackedScene;
            _tankScene = ResourceLoader.Load("res://Assets/UI/Troops/Tank.tscn") as PackedScene;
            _knightScene = ResourceLoader.Load("res://Assets/UI/Troops/Knight.tscn") as PackedScene;
        } 

        private void DisplayStats(int hp, int moves, string troop, string team)
        {
            if(team == "Red")
                GetNode<Sprite>("Sprite").Texture = ResourceLoader.Load("res://Assets/UI/Red UI.png") as Texture;
            else
                GetNode<Sprite>("Sprite").Texture = ResourceLoader.Load("res://Assets/UI/Yellow UI.png") as Texture;
    
            if (GetNode<Position2D>("DisplayPosition").GetChildren().Count != 0)
                GetNode<Position2D>("DisplayPosition").GetChild(0).QueueFree();

            
            var entity = GetInstance(troop).Instance() as Display;
            GetNode<Label>("Position2D/HpLabel").Text = $"{hp}";
            GetNode<Label>("Position2D2/DmgLabel").Text = $"{entity.Damage}";
            GetNode<Label>("Position2D3/MoveLabel").Text = $"{moves}";
            GetNode<Position2D>("DisplayPosition").AddChild(entity);
        }

        private PackedScene GetInstance(string type)
        {
            switch (type)
            {
                case "basic":
                {
                    return _basicScene;
                }
                case "archer":
                {
                    return _archerScene;
                }
                case "wizard":
                {
                    return _wizardScene;
                }
                case "tank":
                {
                    return _tankScene;
                }
                case "knight":
                {
                    return _knightScene;
                }
            }
            return null;
        }
    }
}
