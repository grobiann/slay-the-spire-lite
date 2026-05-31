using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace STSLite.Core.Maps
{
    public abstract class ActMap
    {
        public abstract MapPoint BossMapPoint { get; }
        public abstract MapPoint StartingMapPoint { get; }
        protected abstract MapPoint?[,] Grid { get; }

        public int GetColumnCount() => Grid.GetLength(0);
        public int GetRowCount() => Grid.GetLength(1);

        public IEnumerable<MapPoint> GetAllMapPoints()
        {
            int rowCount = GetRowCount();
            int columnCount = GetColumnCount();

            for (int column = 0; column < columnCount; column++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    MapPoint mapPoint = Grid[column, row];
                    if (mapPoint != null)
                    {
                        yield return mapPoint;
                    }
                }
            }
        }

        public IEnumerable<MapPoint> GetPointsInRow(int row)
        {
            int rowCount = GetRowCount();
            int columnCount = GetColumnCount();

            if (row < 0 || row >= rowCount)
            {
                yield break;
            }

            for (int column = 0; column < columnCount; column++)
            {
                MapPoint mapPoint = Grid[column, row];
                if (mapPoint != null)
                {
                    yield return mapPoint;
                }
            }
        }

        public MapPoint? GetPoint(Vector2Int coord)
        {
            return GetPoint(coord.x, coord.y);
        }

        protected MapPoint? GetPoint(int column, int row)
        {
            int rowCount = GetRowCount();
            int columnCount = GetColumnCount();

            if (rowCount == BossMapPoint.Coord.Column && columnCount == BossMapPoint.Coord.Row)
            {
                return BossMapPoint;
            }
            else if (rowCount == StartingMapPoint.Coord.Column && columnCount == StartingMapPoint.Coord.Row)
            {
                return StartingMapPoint;
            }
            else if (column >= 0 && column < columnCount && row >= 0 && row < rowCount)
            {
                return Grid[column, row];
            }

            return null;
        }
    }
}
