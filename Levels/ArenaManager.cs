using System.Collections.Generic;
using System;
using System.IO;
using CodeBlitz.Objects;
using Godot;

namespace CodeBlitz.Levels
{
    public class ArenaManager : Node2D
    {
        List<Entity> YellowTeam = new List<Entity>();
        List<Entity> RedTeam = new List<Entity>();
        private List<string> _troopNames = new List<string>();
        private PackedScene _archerScene;
        private PackedScene _basicScene;
        private PackedScene _tankScene;
        private PackedScene _wizardScene;
        private PackedScene _knightScene;
        private List<char> _inputList = new List<char>();
        private PackedScene _smokeScene;
        private Random _random = new Random();
        private string _attacker = "Yellow";
        private string _defender = "Red";
        private WorldManager _worldManager;
        private Highlight _worldHighlight;
        private PlayerHighlight _playerHighlight;
        private Entity previousEntity;
        private int _troopsMoved = 0;
        private int _troopsSpawned = 0;
        private bool isSpawn = true;
        private string _currentTeamSpawning = "Yellow";
        [Signal] public delegate void DisplayTroopList(string team);
        [Signal] public delegate void UpdateTroopList(string team);
        [Signal] public delegate void GameOver(string team);
        [Signal] public delegate void DisplayTroopStats(int hp, int moves, string troop, string team);
        public override void _Ready()
        {
            GD.Print("Bruh");
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

            StreamReader sr = new StreamReader("Troop Names.txt");
            string line;
            while ((line = sr.ReadLine()) != null)
                _troopNames.Add(line);
            sr.Close();
        }
        public override void _Input(InputEvent @event)
        {
            //Spawn code
            if(isSpawn && @event is InputEventMouse eventKey && eventKey.IsPressed())
            {
                var position = _worldManager.WorldToMap(eventKey.Position);
                if(!_worldManager.ExistsInArray(position, _worldManager.Obstacles) 
                && !_worldManager.ExistsInArray(position, _worldManager.Players))
                {
                    Entity entity;
                    Vector2 half = (_worldManager.CellSize / 2) / 2;
                    string name = _troopNames[_random.Next(0, _troopNames.Count - 1)];
                    _troopNames.Remove(name);
                    if(_currentTeamSpawning == "Yellow")
                    {
                        entity = GetInstance(Troops.YellowTeam[_troopsSpawned]).Instance() as Entity;
                        entity.Name = Troops.YellowTeam[_troopsSpawned];
                        YellowTeam.Add(entity);
                    }
                    else
                    {
                        entity = GetInstance(Troops.RedTeam[_troopsSpawned]).Instance() as Entity;
                        entity.Name = Troops.RedTeam[_troopsSpawned];
                        RedTeam.Add(entity);
                    }

                    entity.Name = name;
                    entity.GetNode<Label>("NameLabel").Text = entity.Name;
                    entity.Connect(nameof(Entity.EndTurn), this, nameof(EndTurn));
                    entity.Connect(nameof(Entity.InvalidMove), this, nameof(InvalidMove));
                    entity.Connect(nameof(Entity.Death), this, nameof(_on_Entity_Death));
                    entity.Position = _worldManager.MapToWorld(position);
                    entity.Position += new Vector2(half.x / 2, half.y + half.y);

                    var smoke = _smokeScene.Instance() as Smoke;
                    smoke.Position = _worldManager.MapToWorld(position);
                    smoke.Position += new Vector2(half.x / 2, half.y + half.y);
                    GetNode<AudioStreamPlayer>("AudioPlayer").Play();
                    GetNode<Node2D>($"{_currentTeamSpawning} Team").AddChild(entity);
                    AddChild(smoke);
                    _troopsSpawned++;
                    EmitSignal(nameof(UpdateTroopList), _currentTeamSpawning);
                    if(_troopsSpawned >= 4 && _currentTeamSpawning == "Yellow")
                    {
                        _troopsSpawned = 0;
                        _currentTeamSpawning = "Red";
                        EmitSignal(nameof(DisplayTroopList), "Red");
                    }
                    else if (_troopsSpawned >= 4 && _currentTeamSpawning == "Red")
                    {
                        isSpawn = false;
                        GetNode<AnimationPlayer>("AnimationPlayer").Play("Yellow Alert");
                    }
                }
            }
        }

        private void InvalidMove()
        {
            _worldHighlight.Clear();
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
        }

        private void _on_Screen_Enter(string token1, string token2, string token3)
        {
            if (token1 == "move")
            {
                var entity = GetEntity(token2, _attacker);
                if(entity != null)
                {
                    if(entity.MoveCount > 0)
                    {
                        var vector = StringToVector(token3);
                        entity.Move(_worldManager.MapToWorld(vector));
                    }
                }
            }
            else if (token1 == "attack")
            {
                var attacker = GetEntity(token2, _attacker);
                var defender = GetEntity(token3, _defender);
                if(attacker != null && defender != null)
                {
                    if(attacker.MoveCount > 0)
                        attacker.Attack(defender);
                }
                _worldHighlight.Clear();
            }
            else if (token1 == "endturn")
            {
                var entity = GetEntity(token2, _attacker);
                if(entity != null)
                    entity.EmitSignal(nameof(Entity.EndTurn));
            }
            else if (token1 == "select")
            {
                var entity = GetEntity(token3, _attacker); 
                if(entity != null && token2 == "move")
                {
                    _worldHighlight.Clear();
                    entity.ShowArrow();
                    entity.ModulateMoveTiles();
                    previousEntity = entity;
                }

                else if(entity != null && token2 == "attack")
                {
                    _worldHighlight.Clear();
                    entity.ShowArrow();
                    entity.ModulateAtkTiles();
                    previousEntity = entity;
                }
            }

            if (previousEntity != null)
                previousEntity.HideArrow();
            
        }

        private void _end_Game(string team)
        {
            _worldHighlight.Clear();
            GetNode<ColorRect>("ColorRect").Show();
            Troops.Winner = team;
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
            }
            else if(animationName == "Fade Out")
            {
                GetTree().ChangeScene("res://Levels/UI/Win Screen.tscn");
            }
        }

        private Vector2 StringToVector(string token)
        {
            if(token.Length == 2)
                return new Vector2(CharToAxis(token[0].ToString()), CharToAxis(token[1].ToString()));
            else if (token.Length == 3)
            {
                string newString = token[1].ToString() + token[2].ToString();
                GD.Print(newString);
                return new Vector2(CharToAxis(token[0].ToString()), CharToAxis(newString));
            }
            
            return Vector2.Zero;
        }

        private int CharToAxis(string input)
        {
            switch(input)
            {
                case "a": { return 14; }
                case "b": { return 15; }
                case "c": { return 16; }
                case "d": { return 17; }
                case "e": { return 18; }
                case "f": { return 19; }
                case "g": { return 20; }
                case "h": { return 21; }
                case "i": { return 22; }
                case "j": { return 23; }
                case "k": { return 24; }
                case "l": { return 25; }
                case "m": { return 26; }
                case "n": { return 27; }
                case "o": { return 28; }
                case "p": { return 29; }
                
                case "1": { return -5; }
                case "2": { return -4; }
                case "3": { return -3; }
                case "4": { return -2; }
                case "5": { return -1; }
                case "6": { return 0; }
                case "7": { return 1; }
                case "8": { return 2; }
                case "9": { return 3; }
                case "10": { return 4; }
                case "11": { return 5; }
                case "12": { return 6; }
                case "13": { return 7; }
                case "14": { return 8; }
                case "15": { return 9; }
                case "16": { return 10; }
            }
            return 0;
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

        private Entity GetEntity(string name, string team)
        {
            if(team == "Red")
            {
                foreach (var t in RedTeam)
                {
                    if (t.Name == name) return t;
                }
            }
            else
            {
                foreach (var t in YellowTeam)
                {
                   if (t.Name == name) return t;
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
        }
    }
}
