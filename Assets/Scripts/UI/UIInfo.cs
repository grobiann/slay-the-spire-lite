namespace STSLite.UI
{
    public class UIInfo
    {
        public UIInfo(string prefabPath, int sortOrder)
        {
            PrefabPath = prefabPath;
            SortOrder = sortOrder;
        }

        public string PrefabPath { get; }
        public int SortOrder { get; }
    }
}