using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

namespace STSLite.Core.Maps
{
    public class MapPoint : IComparable<MapPoint>
    {
        public MapCoord Coord { get; private set; }
        public readonly HashSet<MapPoint> Parents;
        public readonly HashSet<MapPoint> Children;
        public MapPointType PointType { get; set; }
        public bool CanBeModified { get; set; } = true;

        public bool Equals(MapPoint other)
        {
            return Coord.Equals(other.Coord);
        }

        public int CompareTo(MapPoint other)
        {
            if (other == null)
            {
                return 1;
            }

            return Coord.CompareTo(other.Coord);
        }

        public MapPoint(MapCoord coord)
        {
            Coord = coord;
            Parents = new HashSet<MapPoint>();
            Children = new HashSet<MapPoint>();
        }

        public void AddChild(MapPoint child)
        {
            Children.Add(child);
            child.Parents.Add(this);
        }

        public void RemoveChild(MapPoint child)
        {
            Children.Remove(child);
            child.Parents.Remove(this);
        }

        internal void MoveTo(MapCoord coord)
        {
            Coord = coord;
        }

        public bool IsInTheSameRow(MapPoint other)
        {
            return Coord.Row == other.Coord.Row;
        }

        public override string ToString()
        {
            return $"MapPoint({Coord}, {PointType})";
        }
    }
}
