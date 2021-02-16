using System.Collections.Generic;
using CodeBlitz.Objects;
using Godot;

namespace CodeBlitz.Levels
{
    public class ArenaManager : Node2D
    {
        List<Entity> YellowTeam = new List<Entity>();
        List<Entity> RedTeam = new List<Entity>();
        private PackedScene _archerScene;
        private PackedScene _basicScene;
        private PackedScene _tankScene;
        private PackedScene _wizardScene;
        private PackedScene _knightScene;
        private PackedScene _smokeScene;
        private string _attacker = "Yellow";
        private string _defender = "Red";
        private WorldManager _worldManager;
        private Highlight _worldHighlight;
        private PlayerHighlight _playerHighlight;
        private Entity currentEntity;
        private Entity previousEntity;
        private int _troopsMoved = 0;
        private int _troopIndex = 0;
        private bool _canAttack = false;
        private bool _canChoosetarget = false;
        private bool _canChooseLoc = false;
        private bool _canWalk = false;
        private bool _unitSelected = false;
        private string _state = string.Empty;
        private string _currentTeamSpawning = "Yellow";
        [Signal] public delegate void DisplayTroopList(string team);
        [Signal] public delegate void UpdateTroopList(string team);
        [Signal] public delegate void GameOver(string team);
        [Signal] public delegate void DisplayTroopStats(int hp, int moves, string troop, string team);
        public override void _Ready()
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Fade In");
            _playerHighlight = GetNode<PlayerHighlight>("PlayerHighlight");
            _worldHighlight = GetNode<Highlight>("WorldHighlight");
            _worldManager = GetNode<WorldManager>("World");

            _archerScene = ResourceLoader.Load<PackedScene>("res://Objects/Archer.tscn");
            _basicScene = ResourceLoader.Load<PackedScene>("res://Objects/Basic.tscn");
            _tankScene = ResourceLoader.Load<PackedScene>("res://Objects/Tank.tscn");
            _wizardScene = ResourceLoader.Load<PackedScene>("res://Objects/Wizard.tscn");
            _knightScene = ResourceLoader.Load<PackedScene>("res://Objects/Knight.tscn");
            _smokeScene = ResourceLoader.Load<PackedScene>("res://Objects/Smoke.tscn");
            Connect(nameof(GameOver), this, nameof(_end_Game));
        }

        private void InvalidMove()
        {
            ResetTiles();
            GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
        }

        private void EndTurn()
        {
            _troopsMoved++;
            _worldHighlight.Clear();
            if (_attacker == "Yellow")
            {
                if(_troopsMoved == YellowTeam.Count) 
                {
                    _attacker = "Red";
                    _defender = "Yellow";
                    ResetRound();
                }
            }

            else
            {
                if(_troopsMoved == RedTeam.Count) 
                {
                    _attacker = "Yellow";
                    _defender = "Red";
                    ResetRound();
                }
            }
            GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
            ResetTiles();
        }

        public override void _Input(InputEvent @event)
        {
            if(@event is InputEventMouseButton eventKey)
            {
                if(eventKey.IsActionReleased("L_mouse_down"))
                {
                    var position = _worldManager.WorldToMap(eventKey.Position);
                    if(_state == "Game")
                    {
                        if(!_canAttack && !_canWalk)
                        {
                            GD.Print("Selected Troop");
                            currentEntity = GetEntity(position, _attacker);
                            if(currentEntity != null && currentEntity.MoveCount > 0)
                            {
                                if(previousEntity != null) 
                                    previousEntity.HideArrow();
        
                                currentEntity.ShowArrow();
                                _unitSelected = true; 
                                previousEntity = currentEntity;
                                GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = false;
                                GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = false;
                                GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = false;
                            }
                            else
                            {
                                GD.Print("Cannot Select Entity");
                                ResetTiles();
                                GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
                                GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
                                GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
                            }

                            if(currentEntity != null)
                                EmitSignal(nameof(DisplayTroopStats), currentEntity.Health, currentEntity.MoveCount, currentEntity.Name, currentEntity.Team);
                        
                        }

                        Entity nextPossible = GetEntity(position, _attacker);
                        if(nextPossible != null)
                        {
                            if(position != _worldManager.WorldToMap(currentEntity.Position) && nextPossible.MoveCount > 0)
                            {
                                GD.Print("My Stats");
                                currentEntity = nextPossible;
                                currentEntity.ShowArrow();
                                ResetTiles();
                                previousEntity = nextPossible;
                                _unitSelected = true;
                                EmitSignal(nameof(DisplayTroopStats), currentEntity.Health, currentEntity.MoveCount, currentEntity.Name, currentEntity.Team);
                                GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = false;
                                GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = false;
                                GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = false;
                            }
                        }

                        nextPossible = GetEntity(position, _defender);
                        if(nextPossible != null && !_canChoosetarget && !_canChooseLoc)
                        {
                            GD.Print("Enemy Stats");
                            ResetTiles();
                            EmitSignal(nameof(DisplayTroopStats), nextPossible.Health, nextPossible.MoveCount, nextPossible.Name, nextPossible.Team);
                            GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
                            GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
                            GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
                        }

                        else if(_canAttack && currentEntity.MoveCount > 0 && !_canChoosetarget)
                        {
                            GD.Print("Can Choose Target");
                            _canChoosetarget = true;
                        }

                        else if(_canWalk && currentEntity.MoveCount > 0 && !_canChooseLoc)
                        {
                            GD.Print("Can Choose Location");
                            _canChooseLoc = true;
                        }

                        else if(_canChoosetarget && currentEntity.MoveCount > 0)
                        {
                            GD.Print("Attack");
                            Entity e = GetEntity(position, _defender);
                            if (e != null)
                            {
                                currentEntity.Attack(e);
                            }
                            ResetTiles(); 
                            GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
                            GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
                            GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
                        }
                        else if (_canChooseLoc && currentEntity.MoveCount > 0)
                        {
                            GD.Print("Move");
                            currentEntity.Move(eventKey.Position);
                            ResetTiles();
                            GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
                            GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
                            GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
                        }
                    }
                    else if (_state == "Spawn")
                    {
                        Entity entity;
                        Vector2 half = (_worldManager.CellSize / 2) / 2;;
                        if(!_worldManager.ExistsInArray(position, _worldManager.Obstacles) && !_worldManager.ExistsInArray(position, _worldManager.Players))
                        {
                            if(_currentTeamSpawning == "Yellow")
                            {
                                entity = GetInstance(Troops.YellowTeam[_troopIndex]).Instance() as Entity;
                                entity.Name = Troops.YellowTeam[_troopIndex];
                                YellowTeam.Add(entity);
                            }
                        
                            else
                            {
                                entity = GetInstance(Troops.RedTeam[_troopIndex]).Instance() as Entity;
                                entity.Name = Troops.RedTeam[_troopIndex];
                                RedTeam.Add(entity);
                            }
                        
                            entity.Connect(nameof(Entity.EndTurn), this, nameof(EndTurn));
                            entity.Connect(nameof(Entity.InvalidMove), this, nameof(InvalidMove));
                            entity.Connect(nameof(Entity.Death), this, nameof(_on_Entity_Death));

                            var _half = (_worldManager.CellSize / 2) / 2;
                            entity.Position = _worldManager.MapToWorld(position);
                            entity.Position += new Vector2(half.x / 2, half.y + half.y);
                            //Play cool effect when spawning
                            var smoke = _smokeScene.Instance() as Smoke;
                            smoke.Position = _worldManager.MapToWorld(position);
                            smoke.Position += new Vector2(half.x / 2, half.y + half.y);
                            GetNode<AudioStreamPlayer>("AudioPlayer").Play();
                            GetNode<Node2D>($"{_currentTeamSpawning} Team").AddChild(entity);
                            AddChild(smoke);
                            _troopIndex++;
                            EmitSignal(nameof(UpdateTroopList), _currentTeamSpawning);
                            if(_troopIndex >= 4 && _currentTeamSpawning == "Yellow")
                            {
                                _troopIndex = 0;
                                _currentTeamSpawning = "Red";
                                EmitSignal(nameof(DisplayTroopList), "Red");
                            }
                            else if (_troopIndex >= 4 && _currentTeamSpawning == "Red")
                            {
                                _state = "Game";
                                GetNode<AnimationPlayer>("AnimationPlayer").Play("Yellow Alert");
                            }
                        }
                    }
                }
            }
        }

        private void _on_Attack_Button_down()
        {
            if(_unitSelected)
            {
                currentEntity.ModulateAtkTiles();
                _canAttack = true;
                _canWalk = false;
                _canChooseLoc = false;
            }
        }

        private void _on_Walk_Button_down()
        {
            if(_unitSelected)
            {
                currentEntity.ModulateMoveTiles();
                _canAttack = false;
                _canWalk = true;
                _canChoosetarget = false;
            }
        }

        private void _on_End_Button_down()
        {
            if (_unitSelected)
            {
                currentEntity.MoveCount = 0;
                currentEntity.EmitSignal(nameof(Entity.EndTurn));
            }
        }

        private void _end_Game(string team)
        {
            ResetTiles();
            GetNode<ColorRect>("ColorRect").Show();
            Troops.Winner = team;
            GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Fade Out");

        }

        private void _on_Entity_Death(Entity e, string team)
        {
            if(team == "Red") RedTeam.Remove(e);
            else YellowTeam.Remove(e);

            if(YellowTeam.Count == 0 && RedTeam.Count == 0) EmitSignal(nameof(GameOver), "Tie!");
            else if(YellowTeam.Count == 0) EmitSignal(nameof(GameOver), "Red");
            else if(RedTeam.Count == 0) EmitSignal(nameof(GameOver), "Yellow");
        }

        private void _on_AnimationPlayer_finished(string animationName)
        {
            if (animationName == "Fade In")
            {
                GetNode<ColorRect>("ColorRect").Hide();
                _state = "Spawn";
            }
            else if(animationName == "Fade Out")
            {
                GetTree().ChangeScene("res://Levels/UI/Win Screen.tscn");
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

        private Entity GetEntity(Vector2 position, string team)
        {
            if(team == "Red")
            {
                foreach (var t in RedTeam)
                {
                    if (_worldManager.WorldToMap(t.Position) == position) return t;
                }
            }
            else
            {
                foreach (var t in YellowTeam)
                {
                    if (_worldManager.WorldToMap(t.Position) == position) return t;
                }
            }
            return null;
        }

        private void ResetRound()
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Play($"{_attacker} Alert");
            _troopsMoved = 0;
            foreach (Entity e in RedTeam)
            {
                e.MoveCount = e.MaxMoveCount;
            }
            foreach (Entity e in YellowTeam)
            {
                e.MoveCount = e.MaxMoveCount;
            }
            GetNode<TextureButton>("Screen/Buttons/Attack Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/Walk Button").Disabled = true;
            GetNode<TextureButton>("Screen/Buttons/End Button").Disabled = true;
        }

        private void ResetTiles()
        {
            GetNode<Control>("Screen/TroopDisplay").Hide();
            _worldHighlight.Clear();
            if (previousEntity != null)
            {
                previousEntity.HideArrow();
            } 
            previousEntity = null;
            _unitSelected = false;
            _canChooseLoc = false;
            _canChoosetarget = false;
            _canWalk = false;
            _canAttack = false;
        }
    }
}
