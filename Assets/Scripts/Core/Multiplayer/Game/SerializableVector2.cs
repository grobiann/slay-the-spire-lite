using UnityEngine;

namespace STSLite.Core.Multiplayer.Game
{
    [System.Serializable]
    public struct SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2(Vector2 value)
        {
            x = value.x;
            y = value.y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public static implicit operator Vector2(SerializableVector2 value)
        {
            return new Vector2(value.x, value.y);
        }

        public static implicit operator SerializableVector2(Vector2 value)
        {
            return new SerializableVector2(value);
        }
    }
}