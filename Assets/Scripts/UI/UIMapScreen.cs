using STSLite.Core.Maps;
using STSLite.Core.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace STSLite.UI
{
    public class UIMapScreen : UIBase
    {
        public static UIMapScreen Instance { get; private set; }

        [SerializeField] private UIMapPoint _mapPointPrefab;
        [SerializeField] private UIMapPointConnection _mapPointConnectionPrefab;
        [SerializeField] private RectTransform _mapPointArea;

        private ActMap _map;

        private readonly List<UIMapPoint> _uiMapPoints = new List<UIMapPoint>();
        private readonly List<UIMapPointConnection> _uiMapPointConnections = new List<UIMapPointConnection>();

        public void SetMap(ActMap map, string seed)
        {
            _map = map;
            Canvas.ForceUpdateCanvases();
            _mapPointArea.ForceUpdateRectTransforms();

            foreach (MapPoint point in map.GetAllMapPoints())
            {
                GenerateMapPoint(point);

                foreach (MapPoint child in point.Children)
                {
                    GenerateMapPointConnection(point, child);
                }
            }
        }

        private void GenerateMapPoint(MapPoint mapPoint)
        {
            UIMapPoint uiMapPoint = Instantiate(_mapPointPrefab, _mapPointArea);
            uiMapPoint.transform.localPosition = CalcMapPointPosition(mapPoint);
            uiMapPoint.transform.SetAsLastSibling();
            uiMapPoint.SetMapPoint(mapPoint);
            _uiMapPoints.Add(uiMapPoint);
        }

        private void GenerateMapPointConnection(MapPoint parent, MapPoint child)
        {
            UIMapPointConnection uiMapPointConnection = Instantiate(_mapPointConnectionPrefab, _mapPointArea);
            Vector2 parentPosition = CalcMapPointPosition(parent);
            Vector2 childPosition = CalcMapPointPosition(child);
            Vector2 direction = childPosition - parentPosition;
            uiMapPointConnection.transform.localPosition = parentPosition + direction / 2f;
            uiMapPointConnection.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
            uiMapPointConnection.transform.localScale = new Vector3(direction.magnitude,
                uiMapPointConnection.transform.localScale.y, uiMapPointConnection.transform.localScale.z);
            uiMapPointConnection.transform.SetAsFirstSibling();
            _uiMapPointConnections.Add(uiMapPointConnection);
        }

        private Vector2 CalcMapPointPosition(MapPoint mapPoint)
        {
            Rect areaRect = _mapPointArea.rect;
            Vector2 padding = areaRect.size * 0.05f;
            Rect targetRect = new Rect(
                areaRect.min + padding,
                areaRect.size - padding * 2f);

            float xRate = _map.GetColumnCount() <= 1
                ? 0.5f
                : mapPoint.Coord.Column / (float)(_map.GetColumnCount() - 1);
            float yRate = _map.GetRowCount() <= 1 ? 0.5f : mapPoint.Coord.Row / (float)(_map.GetRowCount() - 1);
            float positionX = targetRect.xMin + xRate * targetRect.width;
            float positionY = targetRect.yMin + yRate * targetRect.height;
            return new Vector2(positionX, positionY);
        }

        public void ClearDrawings()
        {
        }
    }
}