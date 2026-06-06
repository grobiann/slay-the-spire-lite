using NUnit.Framework;
using STSLite.Core.Maps;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace STSLite.UI
{
    [System.Serializable]
    public class MapPointIcon
    {
        public MapPointType mapPointType;
        public Sprite Icon;
    }

    public class UIMapPoint : MonoBehaviour
    {
        [SerializeField] private Image _imageMapType;
        [SerializeField] private List<MapPointIcon> _mapPointIcons;

        public void SetMapPoint(MapPoint mapPoint)
        {
            _imageMapType.sprite = GetMapIcon(mapPoint.PointType);
        }

        public Sprite? GetMapIcon(MapPointType pointType)
        {
            foreach(MapPointIcon icon in _mapPointIcons)
            {
                if(icon.mapPointType == pointType)
                {
                    return icon.Icon;
                }    
            }
            return null;
        }
    }
}
