using System.Threading.Tasks;
using Godot;

namespace CodeBlitz.Objects
{
    public class Archer : Entity
    {
        public override void _Ready()
        {
            base._Ready();
            _moveSpace = new Vector2(5,5);
            _attackSpace = new Vector2(3,3);
        }
        public override void _Process(float delta)
        {
            
        }

        public override async void Attack(Entity e)
        {
            if(!IsOutsideAttack(e.Position) && isOutsideVicinity(_worldManager.WorldToMap(e.Position)))
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

        private bool isOutsideVicinity(Vector2 point)
        {
            var position = _worldManager.WorldToMap(Position);
            return point.x < (position.x - 1) || point.x > (position.x + 1) || point.y < (position.y - 1) || point.y > (position.y + 1);
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

        public override void ModulateAtkTiles()
        {
            _modulate.Clear();
            var position = _worldManager.WorldToMap(Position);
            var yLimit = (int)(position.y - _attackSpace.y) + (_attackSpace.y * 2);
            var xLimit = (int)(position.x - _attackSpace.x) + (_attackSpace.x * 2);
            for(int y = (int)(position.y - _attackSpace.y); y <= yLimit; y++)
            {
                for(int x = (int)(position.x - _attackSpace.x); x <= xLimit; x++)
                {
                    var cell = new Vector2(x, y);
                    if(!IsOutsideMap(cell) && isOutsideVicinity(cell))
                    {
                        if(_worldManager.GetCell(x, y) != 1 && cell != position)
                            _modulate.SetCellv(cell, 1);
                    }
                }
            }
        }
    }
}
