using System.Collections.Generic;
using System.Linq;
using STSLite.UI;
using STSLite;
using STSLite.Core.Entities.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace STSLite.Core.Multiplayer.Game
{
    public class UIRemoteMouseCursorContainer : UIBase
    {
        private PeerInputSynchronizer? _synchronizer;

        [SerializeField] private SlotContainer<UIRemoteMouseCursor> _mouseCursors;

        private void Update()
        {
            if (_synchronizer == null)
            {
                return;
            }

            _synchronizer.SyncLocalMouseDown(Input.GetMouseButtonDown(0));
            _synchronizer.SyncLocalMousePos(Input.mousePosition, transform as RectTransform);
            _synchronizer.SyncLocalIsUsingController(isUsingController: false);

            // if (inputEvent.IsActionReleased(DebugHotkey.hideMpCursors))
            // {
            //     _isDebugUiVisible = !_isDebugUiVisible;
            //     ApplyDebugUiVisibility();
            //     NGame.Instance.AddChildSafely(
            //         NFullscreenTextVfx.Create(_isDebugUiVisible ? "Show MP Cursors" : "Hide MP Cursors"));
            // }

            // if (inputEvent is InputEventMouseMotion inputEventMouseMotion)
            // {
            //     _synchronizer.SyncLocalIsUsingController(isUsingController: false);
            //     if (!NGame.Instance.ReactionWheel.Visible)
            //     {
            //         _synchronizer.SyncLocalMousePos(inputEventMouseMotion.Position, this);
            //     }
            // }
            // else if (inputEvent is InputEventMouseButton inputEventMouseButton)
            // {
            //     _synchronizer.SyncLocalIsUsingController(isUsingController: false);
            //     if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
            //     {
            //         _synchronizer.SyncLocalMouseDown(inputEventMouseButton.Pressed);
            //     }
            // }
        }

        public void Initialize(PeerInputSynchronizer synchronizer, IEnumerable<ulong> connectedPlayerIds)
        {
            if (_synchronizer != null)
            {
                Deinitialize();
            }

            _synchronizer = synchronizer;
            _synchronizer.StateAdded += OnInputStateAdded;
            _synchronizer.StateChanged += OnInputStateChanged;
            _synchronizer.StateRemoved += OnInputStateRemoved;
            _synchronizer.NetService.Disconnected += NetServiceDisconnected;
        }

        public void Deinitialize()
        {
            if (_synchronizer != null)
            {
                _synchronizer.StateAdded -= OnInputStateAdded;
                _synchronizer.StateChanged -= OnInputStateChanged;
                _synchronizer.StateRemoved -= OnInputStateRemoved;
                _synchronizer.NetService.Disconnected -= NetServiceDisconnected;
                _synchronizer.Dispose();
                _synchronizer = null;
            }

            _mouseCursors.SetSize(0);
        }

        private void OnInputStateAdded(ulong playerId)
        {
            AddCursor(playerId);
        }

        private void OnInputStateRemoved(ulong playerId)
        {
            RemoveCursor(playerId);
        }

        private void OnInputStateChanged(ulong playerId)
        {
            // if (playerId == _synchronizer?.NetService.NetId)
            // {
            //     return;
            // }

            UIRemoteMouseCursor cursor = GetCursor(playerId);
            if (cursor)
            {
                Vector2 controlSpaceFocusPosition =
                    _synchronizer.GetControlSpaceFocusPosition(playerId, transform as RectTransform);
                cursor.SetNextPosition(controlSpaceFocusPosition);
                cursor.UpdateImage(_synchronizer.GetMouseDown(playerId));
            }
        }

        private void NetServiceDisconnected(NetErrorInfo _)
        {
            Deinitialize();
        }

        private void AddCursor(ulong playerId)
        {
            if (playerId != _synchronizer?.NetService.NetId)
            {
                UIRemoteMouseCursor cursor = GetCursor(playerId);
                if (cursor)
                {
                    STSLite.Debug.LogError($"Tried to add cursor for player {playerId} twice!");
                    return;
                }

                UIRemoteMouseCursor nRemoteMouseCursor = _mouseCursors.AddNewSlot();
                nRemoteMouseCursor.Setup(playerId);
            }
        }

        private void RemoveCursor(ulong playerId)
        {
            UIRemoteMouseCursor cursor = GetCursor(playerId);
            if (cursor)
            {
                _mouseCursors.RemoveSlot(cursor);
            }
        }

        private UIRemoteMouseCursor? GetCursor(ulong playerId)
        {
            return _mouseCursors.FirstOrDefault((UIRemoteMouseCursor c) => c.PlayerId == playerId);
        }
    }
}