using UnityEngine;

namespace STSLite.Core
{
    public class ResourceManager
    {
    }

    public static class ResourceExtensions
    {
        public static Sprite ToSprite(this string spritePath)
        {
            return Resources.Load<Sprite>(spritePath);
        }
    }
}