using NUnit.Framework;
using STSLite.Core.Models;
using STSLite.Core.Random;
using STSLite.Core.Runs;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace STSLite.Core.Maps
{
    public struct PointTypeCounts
    {
        public int NumOfRests;
        public int NumOfShops;
        public int NumOfElites;
        public int NumOfUnknowns;
    }

    public class StandardActMap : ActMap
    {
        public const int MaxElites = 15;
        public const int MapWidth = 7;

        protected override MapPoint?[,] Grid { get; }
        public override MapPoint BossMapPoint { get; }
        public override MapPoint StartingMapPoint { get; }

        private readonly PointTypeCounts _pointTypeCounts;
        private readonly int _mapHeight;
        private Rng _rng;

        public StandardActMap(ActDefinition actDefinition, Rng mapRng)
        {
            _mapHeight = actDefinition.MapHeight;
            Grid = new MapPoint?[MapWidth, _mapHeight];
            _rng = mapRng;
            _pointTypeCounts = new PointTypeCounts()
            {
                NumOfRests = 2,
                NumOfShops = 1,
                NumOfElites = 3,
                NumOfUnknowns = 15,
            };

            BossMapPoint = GetOrCreateMapPoint(MapWidth / 2, _mapHeight - 1);
            StartingMapPoint = GetOrCreateMapPoint(MapWidth / 2, 0);

            GenerateMapPoints();
            AssignPointTypes();
            PruneAndRepair();
            CenterizeGrid();
            SpreadAdjacentMapPoints();
            StraightenPaths();
            PruneAndRepair();
        }

        private void GenerateMapPoints()
        {
            HashSet<MapPoint> startingPoints = new HashSet<MapPoint>() { StartingMapPoint };
            const int pathCount = 7;
            for (int i = 0; i < pathCount; i++)
            {
                MapPoint startingPoint = GetOrCreateMapPoint(_rng.NextInt(0, GetColumnCount()), 1);
                startingPoints.Add(startingPoint);
                GeneratePath(startingPoint);
            }

            // 보스맵 바로 전의 행에 있는 맵 포인트들을 모두 시작점으로 해서 경로를 생성한다.
            ForEachInRow(Grid, BossMapPoint.Coord.Row - 1, mapPoint => { mapPoint.AddChild(BossMapPoint); });
            ForEachInRow(Grid, 1, mapPoint => { StartingMapPoint.AddChild(mapPoint); });
        }

        private static void ForEachInRow(MapPoint?[,] grid, int row, System.Action<MapPoint> processor)
        {
            int columnCount = grid.GetLength(0);
            for (int column = 0; column < columnCount; column++)
            {
                MapPoint? mapPoint = grid[column, row];
                if (mapPoint != null)
                {
                    processor(mapPoint);
                }
            }
        }

        private MapPoint GetOrCreateMapPoint(int column, int row)
        {
            MapPoint? mapPoint = Grid[column, row];
            if (mapPoint == null)
            {
                mapPoint = new MapPoint(new MapCoord(column, row));
                Grid[column, row] = mapPoint;
            }

            return mapPoint;
        }

        private void GeneratePath(MapPoint startingPoint)
        {
            MapPoint currentPoint = startingPoint;
            while (currentPoint.Coord.Row < BossMapPoint.Coord.Row - 1)
            {
                // Generate next coord
                MapCoord nextCoord;
                {
                    int column = currentPoint.Coord.Column + _rng.NextInt(-1, 2);
                    column = Mathf.Clamp(column, 0, GetColumnCount() - 1);
                    nextCoord = new MapCoord(column, currentPoint.Coord.Row + 1);
                }

                MapPoint nextMapPoint = GetOrCreateMapPoint(nextCoord.Column, nextCoord.Row);
                currentPoint.AddChild(nextMapPoint);
                currentPoint = nextMapPoint;
            }
        }

        private void AssignPointTypes()
        {
            ForEachInRow(Grid, BossMapPoint.Coord.Row - 1, delegate(MapPoint p)
            {
                p.PointType = MapPointType.Rest;
                p.CanBeModified = false;
            });
            ForEachInRow(Grid, 1, delegate(MapPoint p)
            {
                p.PointType = MapPointType.NormalMonster;
                p.CanBeModified = false;
            });

            // Register point types to be assigned in a queue
            Queue<MapPointType> pointTypesToBeAssigned = new Queue<MapPointType>();
            for (int num = 0; num < _pointTypeCounts.NumOfRests; num++)
            {
                pointTypesToBeAssigned.Enqueue(MapPointType.Rest);
            }

            for (int num2 = 0; num2 < _pointTypeCounts.NumOfShops; num2++)
            {
                pointTypesToBeAssigned.Enqueue(MapPointType.Shop);
            }

            for (int num3 = 0; num3 < _pointTypeCounts.NumOfElites; num3++)
            {
                pointTypesToBeAssigned.Enqueue(MapPointType.EliteMonster);
            }

            for (int num4 = 0; num4 < _pointTypeCounts.NumOfUnknowns; num4++)
            {
                pointTypesToBeAssigned.Enqueue(MapPointType.Unknown);
            }

            // Assign remaining types to random points
            {
                List<MapPoint> unassignedMapPoints = (from p in GetAllMapPoints()
                    where p.PointType == MapPointType.Unassigned
                    select p).ToList();
                _rng.Shuffle(unassignedMapPoints);

                foreach (MapPoint mapPoint in unassignedMapPoints)
                {
                    mapPoint.PointType = pointTypesToBeAssigned.Dequeue();
                    if (pointTypesToBeAssigned.Count == 0)
                    {
                        break;
                    }
                }
            }

            foreach (MapPoint mapPoint in GetAllMapPoints())
            {
                if (mapPoint.PointType == MapPointType.Unassigned)
                {
                    mapPoint.PointType = MapPointType.NormalMonster;
                }
            }

            BossMapPoint.PointType = MapPointType.BossMonster;
            StartingMapPoint.PointType = MapPointType.Ancient;
        }

        private void PruneAndRepair()
        {
            for (int row = 0; row < GetRowCount() - 1; row++)
            {
                bool repaired;
                do
                {
                    repaired = false;
                    List<(MapPoint Parent, MapPoint Child)> edges = GetEdgesBetweenRows(row, row + 1);

                    for (int i = 0; i < edges.Count; i++)
                    {
                        for (int j = i + 1; j < edges.Count; j++)
                        {
                            MapPoint parentA = edges[i].Parent;
                            MapPoint childA = edges[i].Child;
                            MapPoint parentB = edges[j].Parent;
                            MapPoint childB = edges[j].Child;

                            if (parentA == parentB || childA == childB)
                            {
                                continue;
                            }

                            if (AreEdgesCrossing(parentA, childA, parentB, childB) == false)
                            {
                                continue;
                            }

                            parentA.RemoveChild(childA);
                            parentB.RemoveChild(childB);
                            parentA.AddChild(childB);
                            parentB.AddChild(childA);
                            repaired = true;
                            break;
                        }

                        if (repaired)
                        {
                            break;
                        }
                    }
                } while (repaired);
            }
        }

        private void CenterizeGrid()
        {
            for (int row = 1; row < GetRowCount() - 1; row++)
            {
                List<MapPoint> points = GetPointsInRow(row).OrderBy(p => p.Coord.Column).ToList();
                if (points.Count == 0)
                {
                    continue;
                }

                int left = points.First().Coord.Column;
                int right = points.Last().Coord.Column;
                int span = right - left;
                int targetLeft = (GetColumnCount() - 1 - span) / 2;
                int shift = targetLeft - left;
                if (shift == 0)
                {
                    continue;
                }

                List<int> targetColumns = points.Select(p => p.Coord.Column + shift).ToList();
                if (CanPlaceRow(points, targetColumns))
                {
                    RepositionRow(points, targetColumns);
                }
            }
        }

        private void SpreadAdjacentMapPoints()
        {
            for (int row = 1; row < GetRowCount() - 1; row++)
            {
                List<MapPoint> points = GetPointsInRow(row).OrderBy(p => p.Coord.Column).ToList();
                if (points.Count <= 1)
                {
                    continue;
                }

                bool hasCluster = false;
                for (int i = 1; i < points.Count; i++)
                {
                    if (points[i].Coord.Column - points[i - 1].Coord.Column <= 1)
                    {
                        hasCluster = true;
                        break;
                    }
                }

                if (hasCluster == false)
                {
                    continue;
                }

                List<int> targetColumns = GetSpreadColumns(points);
                if (CanPlaceRow(points, targetColumns))
                {
                    RepositionRow(points, targetColumns);
                }
            }
        }

        private void StraightenPaths()
        {
            for (int row = 1; row < GetRowCount() - 1; row++)
            {
                List<MapPoint> points = GetPointsInRow(row).ToList();
                foreach (MapPoint point in points)
                {
                    if (point.Parents.Count != 1 || point.Children.Count != 1)
                    {
                        continue;
                    }

                    MapPoint parent = point.Parents.First();
                    MapPoint child = point.Children.First();
                    if (parent.Children.Count != 1 || child.Parents.Count != 1)
                    {
                        continue;
                    }

                    if (parent.Coord.Column != child.Coord.Column || point.Coord.Column == parent.Coord.Column)
                    {
                        continue;
                    }

                    MovePointIfPossible(point, parent.Coord.Column);
                }
            }
        }

        private List<(MapPoint Parent, MapPoint Child)> GetEdgesBetweenRows(int parentRow, int childRow)
        {
            List<(MapPoint Parent, MapPoint Child)> edges = new List<(MapPoint Parent, MapPoint Child)>();
            foreach (MapPoint parent in GetPointsInRow(parentRow))
            {
                foreach (MapPoint child in parent.Children)
                {
                    if (child.Coord.Row == childRow)
                    {
                        edges.Add((parent, child));
                    }
                }
            }

            return edges;
        }

        private static bool AreEdgesCrossing(MapPoint parentA, MapPoint childA, MapPoint parentB, MapPoint childB)
        {
            int parentColumnDelta = parentA.Coord.Column - parentB.Coord.Column;
            int childColumnDelta = childA.Coord.Column - childB.Coord.Column;
            return parentColumnDelta * childColumnDelta < 0;
        }

        private List<int> GetSpreadColumns(List<MapPoint> points)
        {
            int pointCount = points.Count;
            int desiredSpan = Mathf.Min(GetColumnCount() - 1, (pointCount - 1) * 2);
            int currentLeft = points.First().Coord.Column;
            int currentRight = points.Last().Coord.Column;
            float currentCenter = (currentLeft + currentRight) * 0.5f;
            int left = Mathf.RoundToInt(currentCenter - desiredSpan * 0.5f);
            left = Mathf.Clamp(left, 0, GetColumnCount() - 1 - desiredSpan);

            List<int> columns = new List<int>();
            if (pointCount == 1)
            {
                columns.Add(left);
                return columns;
            }

            for (int i = 0; i < pointCount; i++)
            {
                int column = left + Mathf.RoundToInt(i * desiredSpan / (float)(pointCount - 1));
                columns.Add(column);
            }

            return columns;
        }

        private bool CanPlaceRow(List<MapPoint> points, List<int> targetColumns)
        {
            if (points.Count != targetColumns.Count || targetColumns.Distinct().Count() != targetColumns.Count)
            {
                return false;
            }

            int row = points[0].Coord.Row;
            HashSet<MapPoint> movingPoints = new HashSet<MapPoint>(points);
            foreach (int column in targetColumns)
            {
                if (column < 0 || column >= GetColumnCount())
                {
                    return false;
                }

                MapPoint? existingPoint = Grid[column, row];
                if (existingPoint != null && movingPoints.Contains(existingPoint) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private void RepositionRow(List<MapPoint> points, List<int> targetColumns)
        {
            int row = points[0].Coord.Row;
            foreach (MapPoint point in points)
            {
                Grid[point.Coord.Column, row] = null;
            }

            for (int i = 0; i < points.Count; i++)
            {
                MapPoint point = points[i];
                point.MoveTo(new MapCoord(targetColumns[i], row));
                Grid[targetColumns[i], row] = point;
            }
        }

        private void MovePointIfPossible(MapPoint point, int targetColumn)
        {
            int row = point.Coord.Row;
            if (targetColumn < 0 || targetColumn >= GetColumnCount())
            {
                return;
            }

            MapPoint? existingPoint = Grid[targetColumn, row];
            if (existingPoint != null && existingPoint != point)
            {
                return;
            }

            Grid[point.Coord.Column, row] = null;
            point.MoveTo(new MapCoord(targetColumn, row));
            Grid[targetColumn, row] = point;
        }
    }
}