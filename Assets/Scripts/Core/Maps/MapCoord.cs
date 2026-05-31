using System;

namespace STSLite.Core.Maps
{
    [System.Serializable]
    public struct MapCoord : IEquatable<MapCoord>, IComparable<MapCoord>
    {
        public int Column { get; }
        public int Row { get; }

        public readonly int X => Column;
        public readonly int Y => Row;

        public MapCoord(int column, int row)
        {
            Column = column;
            Row = row;
        }
        public override bool Equals(object obj)
        {
            if (obj is MapCoord other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(MapCoord other)
        {
            return Column == other.Column && Row == other.Row;
        }

        public int CompareTo(MapCoord other)
        {
            return (Column, Row).CompareTo((other.Column, other.Row));
        }

        public override int GetHashCode()
        {
            return (Column, Row).GetHashCode();
        }

        public override string ToString()
        {
            return $"MapCoord ({Column}, {Row})";
        }

    }
}
