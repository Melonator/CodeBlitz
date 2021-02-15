using Godot;
using System;
using CodeCombat.World;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeCombat.Entity
{
    public class Entity : Position2D
    {
        [Export] 
        public int Health;
        [Export]
        public int Damage;
        [Export]
        public int MaxMoveCount;
        public int MoveCount;
        
        public int PlayerTile;
        protected Vector2 _targetPosition;
        protected Timer _movementTimer;
        protected WorldManager _worldManager;
        protected TileMap _modulate;
        protected TileMap _playerModulate;
        protected List<Vector2> _path;
        protected Vector2 _targetWorld;
        protected Vector2 _startWorld;
        protected Vector2 _moveSpace;
        protected Vector2 _attackSpace;
        private AnimationPlayer _animPlayer;
        public string Team;
        public new string Name;
        [Signal] public delegate void EndTurn();
        [Signal] public delegate void InvalidMove();
        [Signal] public delegate void Death(Entity e, string team);
        public override void _Ready()
        {
            MoveCount = MaxMoveCount;
            GetNode<AnimatedSprite>("AnimatedSprite").Play("idle");
            GetNode<AnimationPlayer>("ArrowNode/AnimationPlayer").Play("Bounce");
            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _modulate = (Highlight)GetParent().GetParent().GetNode("WorldHighlight");
            _playerModulate = (PlayerHighlight)GetParent().GetParent().GetNode("PlayerHighlight");
            _movementTimer = GetNode<Timer>("MovementTimer");
            _worldManager = (WorldManager)GetParent().GetParent().GetNode("World");
            _worldManager.SetStartPosition(Position);
            SetColor();
            _playerModulate.SetCellv(_worldManager.WorldToMap(Position), PlayerTile);
        }

        private void SetColor()
        {
            if (GetParent().Name == "Yellow Team")
            {
                Team = "Yellow";
                PlayerTile = 2;
            }
            else
            {
                Team = "Red";
                PlayerTile = 3;
            }
        }

        public async void TakeDamage(int damage)
        {
            Health -= damage;
            GetNode<AudioStreamPlayer>("HitPlayer").Play();
            _animPlayer.Play("Damaged");
            Modulate = Color.ColorN("Red");
			await Task.Delay(200);
            Modulate = new Color(1, 1, 1);
			await Task.Delay(200);
            Modulate = Color.ColorN("Red");
			await Task.Delay(200);
            Modulate = new Color(1, 1, 1);
            Modulate = Color.ColorN("Red");
			await Task.Delay(200);
            Modulate = new Color(1, 1, 1);
            Modulate = Color.ColorN("Red");
			await Task.Delay(200);
            Modulate = new Color(1, 1, 1);
            if(Health <= 0)
            {
                EmitSignal(nameof(Death), this, Team);
                _playerModulate.SetCellv(_playerModulate.WorldToMap(Position), -1);
                _worldManager.SetCellv(_playerModulate.WorldToMap(Position), -1);
                _worldManager.RefreshTiles();
                QueueFree();
            }
        }

        public void ShowArrow()
        {
            GetNode<Node2D>("ArrowNode").Show();
        }

        public void HideArrow()
        {
            GetNode<Node2D>("ArrowNode").Hide();
        }

        public virtual void ModulateMoveTiles()
        {
            //Base method for overriding
        } 

        public virtual void ModulateAtkTiles()
        {
            //Base method for overriding
        }

        public virtual async void Attack(Entity e)
        {
            if(!IsOutsideAttack(e.Position) && !IsThereObstacle(e.Position))
            {
                GetNode<AnimatedSprite>("AnimatedSprite").Play("attack");
                e.TakeDamage(Damage);
                await Task.Delay(1000);
                GetNode<AnimatedSprite>("AnimatedSprite").Play("idle");
                MoveCount--;
                if (MoveCount <= 0) EmitSignal(nameof(EndTurn));
            }
            else
            {
                EmitSignal(nameof(InvalidMove));
            }
        }

        private bool IsThereObstacle(Vector2 point)
        {
            var newPoint = _worldManager.WorldToMap(point);
            var newPosition = _worldManager.WorldToMap(Position);
            var difference = newPoint - newPosition;
            int distance = 0;
            if(Math.Abs(difference.y) == Math.Abs(difference.x) || Math.Abs(newPosition.x) == Math.Abs(newPoint.x))
                distance = (int)Math.Abs(difference.y);
            
            else if (Math.Abs(newPosition.y) == Math.Abs(newPoint.y))
                distance = (int)Math.Abs(difference.x);

            return CanAttack(distance, newPosition, newPoint);
        }

        private bool CanAttack(int distance, Vector2 position, Vector2 point)
        {
            var pathType = GetPathType(point, position);
            GD.Print(pathType);
            GD.Print("MY POSITION");
            GD.Print(position);
            GD.Print("ENEMY POSITION");
            GD.Print(point);
            for(int i = 0; i <= distance; i++)
            {
                var nextPossible = CalculateNextPath(pathType, i, position);
                GD.Print("============");
                GD.Print(nextPossible);
                if(_worldManager.ExistsInArray(nextPossible, _worldManager.Obstacles)) return true;
            }
            GD.Print("CAN ATTACK!");
            return false;
        }

        private Vector2 CalculateNextPath(string pathDirection, int tilesPassed, Vector2 point)
        {
            switch(pathDirection)
            {
                case "upperRight":
                {
                    return new Vector2(point.x + tilesPassed, point.y - tilesPassed);
                }
                case "upperLeft":
                {
                    return new Vector2(point.x - tilesPassed, point.y - tilesPassed);
                }
                case "lowerRight":
                {
                    return new Vector2(point.x + tilesPassed, point.y + tilesPassed);
                }
                case "lowerLeft":
                {
                    return new Vector2(point.x - tilesPassed, point.y + tilesPassed);
                }
                case "up":
                {
                    return new Vector2(point.x, point.y - tilesPassed);
                }
                case "down":
                {
                    return new Vector2(point.x, point.y + tilesPassed);
                }
                case "left":
                {
                    return new Vector2(point.x - tilesPassed, point.y);
                }
                case "right":
                {
                    return new Vector2(point.x + tilesPassed, point.y);
                }

            }
            return new Vector2();
        }

        private string GetPathType(Vector2 point, Vector2 position)
        {
            if(point.x == position.x && point.y < position.y)
                return "up";
            else if(point.x == position.x && point.y > position.y)
                return "down";
            else if(point.y == position.y && point.x < position.x)
                return "left";
            else if (point.y == position.y && point.x > position.x)
                return "right";
            else if (point.x > position.x && point.y < position.y)
                return "upperRight";
            else if(point.x < position.x && point.y < position.y)
                return "upperLeft";
            else if(point.x > position.x && point.y > position.y)
                return "lowerRight";
            else 
                return "lowerLeft";
        }

        public bool IsOutsideMove(Vector2 point)
        {
            point = _worldManager.WorldToMap(point);
            var position = _worldManager.WorldToMap(Position);
            return point.y < (position.y - _moveSpace.y) || point.y > (position.y + _moveSpace.y) || point.x < (position.x - _moveSpace.x) || point.x > (position.x + _moveSpace.x);
        }

        public virtual bool IsOutsideAttack(Vector2 point)  
        {
            point = _worldManager.WorldToMap(point);
            var position = _worldManager.WorldToMap(Position);
            return point.y < (position.y - _attackSpace.y) || point.y > (position.y + _attackSpace.y) || point.x < (position.x - _attackSpace.x) || point.x > (position.x + _attackSpace.x);
        }

        public bool IsOutsideMap(Vector2 position)
        {
              return position.x < 13 || position.y < -6 || position.x > 30 || position.y > 11;
        }

        public void Move(Vector2 position)
        {
            if(!IsOutsideMove(position))
            {
                _targetPosition = position;
                _startWorld = Position;
                if(!_worldManager.ExistsInArray(_worldManager.WorldToMap(_targetPosition), _worldManager.Obstacles) && !_worldManager.ExistsInArray(_worldManager.WorldToMap(_targetPosition), _worldManager.Players))
                {
                    MoveCount--;
                    GetNode<AnimatedSprite>("AnimatedSprite").Play("run");
                    _playerModulate.SetCellv(_worldManager.WorldToMap(Position), -1);
                    _worldManager.SetCellv(_worldManager.WorldToMap(Position), -1);
                    _path = _worldManager.FindPath(Position, _targetPosition);
                    if(!(_path.Count <= 1))
                    {
                        _targetWorld = _path[1];
                        _movementTimer.Start();
                    }
                }
                else
                {
                    GetNode<AnimatedSprite>("AnimatedSprite").Play("idle");
                    EmitSignal(nameof(InvalidMove));
                }
                
            }
            else
            {
                GetNode<AnimatedSprite>("AnimatedSprite").Play("idle");
                EmitSignal(nameof(InvalidMove));
            }
            GetNode<AnimatedSprite>("AnimatedSprite").Play("idle");
            EmitSignal(nameof(InvalidMove));
        } 
    }
}
