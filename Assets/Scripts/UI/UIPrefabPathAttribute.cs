using System;

namespace STSLite.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UIPrefabPathAttribute : Attribute
    {
        public UIPrefabPathAttribute(string path, SearchType searchType = SearchType.ProjectPath)
        {
            Path = path;
            SearchType = searchType;
        }

        public string Path { get; }
        public SearchType SearchType { get; }
    }

    public enum SearchType
    {
        ProjectPath,
        ResourcesPath
    }
}