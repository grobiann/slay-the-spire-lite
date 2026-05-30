using UnityEngine;
using System.Text.RegularExpressions;

namespace STSLite.Core
{
    public class CoreUtils
    {
        public static T CreateSingletonObject<T>(string name) where T : MonoBehaviour
        {
            GameObject singletonObject = new GameObject(name);
            T singletonComponent = singletonObject.AddComponent<T>();
            Object.DontDestroyOnLoad(singletonObject);
            return singletonComponent;
        }
    }

    public static class StringHelper
    {
        private static readonly Regex mCamelCaseRegex = new Regex("([a-z0-9])([A-Z])");
        private static readonly Regex mWhitespaceRegex = new Regex("\\s+");
        private static readonly Regex mSpecialCharRegex = new Regex("[^A-Z0-9_]");

        public static string Slugify(string txt)
        {
            string text = mCamelCaseRegex.Replace(txt.Trim(), "$1_$2");
            string input = mWhitespaceRegex.Replace(text.ToUpperInvariant(), "_");
            return mSpecialCharRegex.Replace(input, "");
        }
    }
}
