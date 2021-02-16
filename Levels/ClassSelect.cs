using CodeBlitz.Assets.UI.Troops;
using Godot;

namespace CodeBlitz.Levels
{
    public class ClassSelect : Control
    {

        #region PackedScenes
        private PackedScene _basicUiScene;
        private PackedScene _archerUiScene;
        private PackedScene _wizardUiScene;
        private PackedScene _tankUiScene;
        private PackedScene _knightUiScene;
        private PackedScene _basicScene;
        private PackedScene _archerScene;
        private PackedScene _wizardScene;
        private PackedScene _tankScene;
        private PackedScene _knightScene;

        #endregion
        private string[] _classes = new string[4];
        private Display previousSprite;
        private int _teamCount = 0;
        private int _currentTeam = 0;
        private bool _canPress = false;
        public override void _Ready()
        {
            GetParent().GetNode<AnimationPlayer>("AnimationPlayer").Play("Fade In");
            _basicUiScene = ResourceLoader.Load("res://Levels/UI/Buttons/Basic.tscn") as PackedScene;
            _archerUiScene = ResourceLoader.Load("res://Levels/UI/Buttons/Archer.tscn") as PackedScene;
            _wizardUiScene = ResourceLoader.Load("res://Levels/UI/Buttons/Wizard.tscn") as PackedScene;
            _tankUiScene = ResourceLoader.Load("res://Levels/UI/Buttons/Tank.tscn") as PackedScene;
            _knightUiScene = ResourceLoader.Load("res://Levels/UI/Buttons/Knight.tscn") as PackedScene;

            _basicScene = ResourceLoader.Load("res://Assets/UI/Troops/Basic.tscn") as PackedScene;
            _archerScene = ResourceLoader.Load("res://Assets/UI/Troops/Archer.tscn") as PackedScene;
            _wizardScene = ResourceLoader.Load("res://Assets/UI/Troops/Wizard.tscn") as PackedScene;
            _tankScene = ResourceLoader.Load("res://Assets/UI/Troops/Tank.tscn") as PackedScene;
            _knightScene = ResourceLoader.Load("res://Assets/UI/Troops/Knight.tscn") as PackedScene;
        }

        private void Fill(string type)
        {
            if(_canPress)
            {
                TextureButton button = null;
                for(int i = 1; i <= 4; i++)
                {
                    Position2D s = GetNode<Position2D>($"{i}");
                    if (s.GetChildCount() != 1)
                    {
                        if(_currentTeam == 0) Troops.YellowTeam[i - 1] = type;
                        else Troops.RedTeam[i - 1] = type;
                        _teamCount++;
                        switch (type)
                        {
                            case "basic":
                            {
                                button = _basicUiScene.Instance() as TextureButton;
                                break;
                            }
                            case "archer":
                            {
                                button = _archerUiScene.Instance() as TextureButton;
                                break;
                            }
                            case "wizard":
                            {
                                button = _wizardUiScene.Instance() as TextureButton;
                                break;
                            }
                            case "tank":
                            {
                                button = _tankUiScene.Instance() as TextureButton;
                                break;
                            }
                            case "knight":
                            {
                                button = _knightUiScene.Instance() as TextureButton;
                                break;
                            }
                        }
                        _classes[i - 1] = type;
                        button.Connect("gui_input", this, $"_on_Button{i}_pressed");
                        button.Connect("mouse_entered", this, nameof(_on_button_mouse_entered));
                        s.AddChild(button);
                        break;
                    }
                }
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

        private void SetInformation(int index)
        {
            Display sprite = GetInstance(_classes[index]).Instance() as Display;
            GetNode<Label>("ClassLabel").Text = _classes[index];
            if(GetNode<Position2D>("ClassSprite").GetChildCount() != 0)
                GetNode<Position2D>("ClassSprite").RemoveChild(previousSprite);
        
            GetNode<Label>("DamageLabel").Text = $"{sprite.Damage}";
            GetNode<Label>("HealthLabel").Text = $"{sprite.Health}";
            GetNode<Label>("DescriptionLabel").Text = sprite.Description;
            GetNode<Label>("DescriptionLabel").SetSize( new Vector2(59,33));
            GetNode<Position2D>("ClassSprite").AddChild(sprite);
            previousSprite = sprite;
        }

        #region ButtonEvents

        private void _on_button_mouse_entered()
        {
            GetParent().GetNode<AudioStreamPlayer>("SelectPlayer").Play();
        }
    
        private void _on_AnimationPlayer_finished(string animName)
        {
            if(animName == "Fade In")
            {
                GetParent().GetNode<AnimationPlayer>("AnimationPlayer").Play("Yellow Alert");
                GetParent().GetNode<ColorRect>("ColorRect2").Hide();
            }
        
            else if (animName == "Fade Out")
            {
                GetTree().ChangeScene("res://Levels/NavigationArena.tscn");
            }

            else if(animName == "Yellow Alert" || animName == "Red Alert")
            {
                _canPress = true;
            }
        }

        private void _on_Ok_Button_pressed()
        {
            if(_currentTeam == 0)
            {
                _currentTeam = 1; 
                _teamCount = 0;
                _canPress = false;
                GetNode<TextureButton>("Ok Button").Disabled = true;
                GetNode<Position2D>("1").GetChild(0).QueueFree();
                GetNode<Position2D>("2").GetChild(0).QueueFree();
                GetNode<Position2D>("3").GetChild(0).QueueFree();
                GetNode<Position2D>("4").GetChild(0).QueueFree();
                _canPress = false;
                GetParent().GetNode<AnimationPlayer>("AnimationPlayer").Play("Red Alert");
            }
            else
            {
                GetParent().GetNode<ColorRect>("ColorRect2").Show();
                GetParent().GetNode<AnimationPlayer>("AnimationPlayer").Play("Fade Out");
            }
        }
        private void _on_Basic_pressed()
        {
            Fill("basic");
            CheckTeamCount();
        }

        private void _on_Archer_pressed()
        {
            Fill("archer");
            CheckTeamCount();
        }

        private void _on_Tank_pressed()
        {
            Fill("tank");
            CheckTeamCount();
        }

        private void _on_Wizard_pressed()
        {
            Fill("wizard");
            CheckTeamCount();
        }

        private void _on_Knight_pressed()
        {
            Fill("knight");
            CheckTeamCount();
        }

        private void _on_Button1_pressed(InputEvent @event)
        {
            if (@event.IsActionPressed("R_mouse_down"))
            {
                GetNode<Position2D>("1").GetChild(0).QueueFree();
                _teamCount--;
            }

            else if(@event.IsActionPressed("L_mouse_down"))
            {
                SetInformation(0);
            }
            CheckTeamCount();
        }
    
        private void _on_Button2_pressed(InputEvent @event)
        {
            if (@event.IsActionPressed("R_mouse_down"))
            {
                GetNode<Position2D>("2").GetChild(0).QueueFree();
                _teamCount--;
            }

            else if(@event.IsActionPressed("L_mouse_down"))
            {
                SetInformation(1);
            }
            CheckTeamCount();
        }
        private void _on_Button3_pressed(InputEvent @event)
        {
            if (@event.IsActionPressed("R_mouse_down"))
            {
                GetNode<Position2D>("3").GetChild(0).QueueFree();
                _teamCount--;
            }

            else if(@event.IsActionPressed("L_mouse_down"))
            {
                SetInformation(2);
            }
            CheckTeamCount();
        }
        private void _on_Button4_pressed(InputEvent @event)
        {
            if (@event.IsActionPressed("R_mouse_down"))
            {
                GetNode<Position2D>("4").GetChild(0).QueueFree();
                _teamCount--;
            }

            else if(@event.IsActionPressed("L_mouse_down"))
            {
                SetInformation(3);
            }
            CheckTeamCount();
        }

        private void CheckTeamCount()
        {
            if(_teamCount == 4) GetNode<TextureButton>("Ok Button").Disabled = false;
            else GetNode<TextureButton>("Ok Button").Disabled = true;
        }

        #endregion

    }
}
