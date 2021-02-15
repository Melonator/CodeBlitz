using Godot;
using System;
using CodeCombat.Entity;
using CodeCombat.World;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeCombat.Entity.Wizard
{
    public class Wizard : Entity
    {
        private List<Vector2> attackTiles = new List<Vector2>();
        public override void _Ready()
        {
            base._Ready();
            _moveSpace = new Vector2(3,3);
            _attackSpace = new Vector2(4,4);
        }

        private void _on_MovementTimer_timeout()
        {
            _movementTimer.Stop();
            Position = _targetWorld;
            _path.RemoveAt(0);
            if(_path.Count == 0) 
            {
                GetNode<AnimatedSprite>("AnimatedSprite").Play("idle");
                _playerModulate.SetCellv(_worldManager.WorldToMap(Position), PlayerTile);
                if (MoveCount <= 0) EmitSignal(nameof(EndTurn));
                return;        
            }
            _targetWorld = _path[0];
            _movementTimer.Start();
        }

        public override void ModulateMoveTiles()
        {
            _modulate.Clear();
            var position = _worldManager.WorldToMap(Position);
            var yLimit = (int)(position.y - _moveSpace.y) + (_moveSpace.y * 2);
            var xLimit = (int)(position.x - _moveSpace.x) + (_moveSpace.x * 2);
            for(int y = (int)(position.y - _moveSpace.y); y <= yLimit; y++)
            {
                for(int x = (int)(position.x - _moveSpace.x); x <= xLimit; x++)
                {
                    var cell = new Vector2(x, y);
                    
                    if(!IsOutsideMap(cell))
                    {
                        if(_worldManager.GetCell(x, y) == 1 && cell != position)
                            _modulate.SetCellv(cell, 1);
                        else
                            _modulate.SetCellv(cell, 0);
                    }
                }
            }
        }

        public override async void Attack(Entity e)
        {
            if(!IsOutsideAttack(e.Position))
            {
                //EXPLOOOOSION
                var point = _worldManager.WorldToMap(e.Position);
                var position = _worldManager.WorldToMap(Position);
                Vector2[] points = 
                {
                    new Vector2(position.x + 1, position.y),
                    new Vector2(position.x - 1, position.y),
                    new Vector2(position.x, position.y + 1),
                    new Vector2(position.x, position.y -1)
                };

                GetNode<AnimatedSprite>("AnimatedSprite").Play("attack");
                await Task.Delay(300);
                e.TakeDamage(Damage);
                GetNode<AudioStreamPlayer>("AttackPlayer").Play();
                foreach (var cell in points)
                {
                    if (point == cell)
                    {
                        TakeDamage(Health / 2);
                    }
                }
                await Task.Delay(900);
                GetNode<AnimatedSprite>("AnimatedSprite").Play("idle");
                MoveCount--;
                if (MoveCount <= 0) EmitSignal(nameof(EndTurn));
            }
            else
            {
                EmitSignal(nameof(InvalidMove));
            }
        }

        public override bool IsOutsideAttack(Vector2 point)
        {
            point = _worldManager.WorldToMap(point);
            foreach (var cell in attackTiles)
            {
                if(cell == point) return false;
            }
            return true;
        }

 

        private void InitializeAttackTiles(Vector2 point)
        {
            for(int x = (int)(point.x - _attackSpace.x); x <= (int)(point.x + _attackSpace.x); x++)
            {
                attackTiles.Add(new Vector2(x, point.y));
            }

            for(int y = (int)(point.y - _attackSpace.y); y <= (int)(point.y + _attackSpace.y); y++)
            {
                attackTiles.Add(new Vector2(point.x, y));
            }
        }

        public override void ModulateAtkTiles()
        {
            _modulate.Clear();
            attackTiles.Clear();
            var position = _worldManager.WorldToMap(Position);
            InitializeAttackTiles(position);
            foreach (var cell in attackTiles)
            {
                if(!IsOutsideMap(cell))
                {
                    if(_worldManager.GetCell((int)cell.x, (int)cell.y) != 1 && cell != position)
                        _modulate.SetCellv(cell, 1);
                }
            }
        }
    }
}
