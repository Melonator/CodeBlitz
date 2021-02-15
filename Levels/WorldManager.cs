using Godot;
using System;
using System.Collections.Generic;

namespace CodeCombat.World
{
    public class WorldManager : TileMap
    {
        public AStar AstarNode = new AStar();
        public Vector2 MapSize = new Vector2(19, 19);
        public Vector3[] PointPath;
        public Godot.Collections.Array Obstacles;
        public Godot.Collections.Array Players = new Godot.Collections.Array();
        private Vector2 _startPosition;
        private Vector2 _endPosition; 
        private Vector2 _half;

        public override void _Ready()
        {
           RefreshTiles();
        }
        public void RefreshTiles()
        {
            AstarNode.Clear();
            _half = (CellSize / 2) / 2;
            Players = GetUsedCellsById(2);
            Obstacles = GetUsedCellsById(1);
            var walkableCells = AddWalkableCells();
            ConnectWalkableCells(walkableCells);
        }

        public void SetStartPosition(Vector2 start)
        {
            SetCellv(WorldToMap(start), 2);
            RefreshTiles();
        }

        private List<Vector2> AddWalkableCells()
        {
            List<Vector2> pointPath = new List<Vector2>();
            for (int x = 13; x <= 30; x++)
            {
                for (int y = -6; y <= 11; y++)
                {
                    var point = new Vector2(x, y);
                    if (ExistsInArray(point, Obstacles) || ExistsInArray(point, Players)) continue;
                    int pointIndex = CalculatePointIndex(point);
                    pointPath.Add(point);
                    AstarNode.AddPoint(pointIndex, new Vector3(point.x, point.y, 0.0f));
                }
            }
            return pointPath; 
        }

        private void ConnectWalkableCells(List<Vector2> points_array)
        {
            foreach(var point in points_array)
            {
                int pointIndex = CalculatePointIndex(point);
                Vector2[] pointsRelative = { new Vector2(point.x + 1, point.y),
			        new Vector2(point.x - 1, point.y),
			        new Vector2(point.x, point.y + 1),
			        new Vector2(point.x, point.y - 1)};

                foreach(var relativepoint in pointsRelative)
                {
                    if(IsOutsideMap(relativepoint)) continue;
                    int pointRelativeIndex = CalculatePointIndex(relativepoint);
                    if (!AstarNode.HasPoint(pointRelativeIndex)) continue;
                    AstarNode.ConnectPoints(pointIndex, pointRelativeIndex, false);
                }
            }
        }

        public bool ExistsInArray(Vector2 coordinate, Godot.Collections.Array array)
        {
            for(int i = 0; i < array.Count; i++)
            {
                if ((Vector2)array[i] == coordinate) 
                {
                    return true;
                }
            }
            return false;
        }

        private int CalculatePointIndex(Vector2 point)
        {
            point += MapSize;
            return (int)(point.y + MapSize.x * point.x);
        }

        public bool IsOutsideMap(Vector2 point)
        {
            return point.x < 13 || point.y < -6 || point.x > 30 || point.y > 11;
        }

        public List<Vector2> FindPath(Vector2 world_start, Vector2 world_end)
        {
            RefreshTiles();
            _startPosition = WorldToMap(world_start);
            _endPosition = WorldToMap(world_end);
            List<Vector2> pathWorld = new List<Vector2>();
            RecalculatePath();
            UpdateTile(_startPosition, _endPosition);
            RefreshTiles();
            foreach(var point in PointPath)
            {
                var pointWorld = MapToWorld(new Vector2(point.x, point.y));  
                pointWorld += new Vector2(_half.x / 2, _half.y + _half.y);
                pathWorld.Add(pointWorld);
            } 
            return pathWorld;
        }

        public void UpdateTile(Vector2 world_start, Vector2 world_end)
        {
            SetCellv(world_start, -1);
            SetCellv(world_end, 2);
        }

        private void RecalculatePath()
        {
            var startPointIndex = CalculatePointIndex(_startPosition);
            var endPointIndex = CalculatePointIndex(_endPosition);
            PointPath = AstarNode.GetPointPath(startPointIndex, endPointIndex);
            Update();
        }
    }
}
