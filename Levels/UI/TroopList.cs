using CodeBlitz.Assets.UI.Troops;
using Godot;

namespace CodeBlitz.Levels.UI
{
    public class TroopList : Control
    {
        private PackedScene _basicScene;
        private PackedScene _archerScene;
        private PackedScene _wizardScene;
        private PackedScene _tankScene;
        private PackedScene _knightScene;
        private int _troopLimit = 4;
        public override void _Ready()
        {
            _basicScene = ResourceLoader.Load("res://Assets/UI/Troops/Basic.tscn") as PackedScene;
            _archerScene = ResourceLoader.Load("res://Assets/UI/Troops/Archer.tscn") as PackedScene;
            _wizardScene = ResourceLoader.Load("res://Assets/UI/Troops/Wizard.tscn") as PackedScene;
            _tankScene = ResourceLoader.Load("res://Assets/UI/Troops/Tank.tscn") as PackedScene;
            _knightScene = ResourceLoader.Load("res://Assets/UI/Troops/Knight.tscn") as PackedScene;
            DisplayTroopList("Yellow");
        }

        private void DisplayTroopList(string team)
        {
            if (team == "Red")
            {
                int j = 1;
                for(int i = 3; i >= 0; i--, j++)
                {
                    var entity = GetInstance(Troops.RedTeam[i]).Instance() as Display;
                    GetNode<Position2D>($"{j}").AddChild(entity);
                }
            }

            else
            {
                int j = 1;
                for(int i = 3; i >= 0; i--, j++)
                {
                    var entity = GetInstance(Troops.YellowTeam[i]).Instance() as Display;
                    GetNode<Position2D>($"{j}").AddChild(entity);
                }
            }
        }

        private void UpdateTroopList(string team)
        {
            GetNode<Position2D>($"{_troopLimit}").GetChild(0).QueueFree();
            _troopLimit--;
            if (_troopLimit == 0 && team == "Yellow")
            {
                _troopLimit = 4;
            }

            else if(_troopLimit == 0 && team == "Red")
            {
                QueueFree();
            }
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
