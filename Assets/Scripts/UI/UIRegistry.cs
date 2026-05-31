using System;
using System.Collections.Generic;

namespace STSLite.UI
{
    public static class UIRegistry
    {
        private static readonly Dictionary<Type, UIInfo> mUIInfos = new Dictionary<Type, UIInfo>()
        {
            { typeof(UIMainMenu), new UIInfo("Prefabs/UI/UIMainMenuWindow", 0) },
            { typeof(UITopBar), new UIInfo("Prefabs/UI/TopBar", 10) },
            { typeof(UILogo), new UIInfo("Prefabs/UI/UILogoWindow", 100) },
            { typeof(UIMapScreen), new UIInfo("Prefabs/UI/MapScreen/UIMapScreenWindow", 100) },
            { typeof(UIBlackScreen), new UIInfo("Prefabs/UI/UIBlackScreenWindow", 1000) },
        };

        public static UIInfo GetUIInfo(Type uiType)
        {
            UIInfo uiInfo;

            if (!mUIInfos.TryGetValue(uiType, out uiInfo))
            {
                throw new InvalidOperationException($"{uiType.Name} is not registered in UIRegistry.");
            }

            return uiInfo;
        }
    }
}
