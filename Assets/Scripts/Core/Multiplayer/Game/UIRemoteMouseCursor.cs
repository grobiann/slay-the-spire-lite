using System;
using UnityEngine;

namespace STSLite.Core.Multiplayer.Game
{
    public class UIRemoteMouseCursor : MonoBehaviour
    {
        public ulong PlayerId { get; private set; }
        private Vector2? _previousPosition;
        private Vector2? _nextPosition;
        private float _lastPositionUpdateSec;

        public void Setup(ulong playerId)
        {
            PlayerId = playerId;
        }

        private void Update()
        {
            if (_previousPosition.HasValue && _nextPosition.HasValue)
            {
                float lerpT = Mathf.Clamp01((Time.time - _lastPositionUpdateSec) / 50);
                transform.position = Vector2.Lerp(_previousPosition.Value, _nextPosition.Value, lerpT);
            }
        }

        public void SetNextPosition(Vector2 position)
        {
            if (!_nextPosition.HasValue)
            {
                _nextPosition = position;
            }

            _previousPosition = _nextPosition;
            _nextPosition = position;
            _lastPositionUpdateSec = Time.time;
        }

        public void UpdateImage(bool isMouseDown)
        {
        }
    }
}