using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace STSLite.Core.Multiplayer.Game
{
    public class PeerInputSynchronizer : IDisposable
    {
        private class PeerInputState
        {
            public ulong playerId;
            public Vector2 netMousePosition;
            public bool isMouseDown;
            public bool isUsingController;
            public Vector2 controllerFocusPosition;
            public NetScreenType netScreenType;
        }

        public const int minUpdateMsec = 50;
        private readonly INetGameService _netService;
        private readonly List<PeerInputState> _inputStates = new List<PeerInputState>();
        private float _lastSyncSec;
        private PeerInputMessage? _syncMessageToSend;
        private Task? _syncMessageTask;

        public INetGameService NetService => _netService;
        public event Action<ulong>? StateAdded;
        public event Action<ulong>? StateRemoved;
        public event Action<ulong>? StateChanged;
        public event Action<ulong, NetScreenType>? ScreenChanged;

        public PeerInputSynchronizer(INetGameService netService)
        {
            _netService = netService;
            _netService.RegisterMessageHandler<PeerInputMessage>(HandlePeerInputMessage);
            GetOrCreateStateForPlayer(_netService.NetId);
        }

        public void Dispose()
        {
            _netService.UnregisterMessageHandler<PeerInputMessage>(HandlePeerInputMessage);
        }

        private PeerInputState GetOrCreateStateForPlayer(ulong playerId)
        {
            PeerInputState peerInputState = GetStateForPlayer(playerId);
            if (peerInputState == null)
            {
                peerInputState = new PeerInputState();
                peerInputState.playerId = playerId;
                _inputStates.Add(peerInputState);
                this.StateAdded?.Invoke(playerId);
            }

            return peerInputState;
        }

        private PeerInputState? GetStateForPlayer(ulong playerId)
        {
            int num = _inputStates.FindIndex((PeerInputState s) => s.playerId == playerId);
            if (num >= 0)
            {
                return _inputStates[num];
            }

            return null;
        }

        private PeerInputState ForceGetStateForPlayer(ulong playerId)
        {
            return GetStateForPlayer(playerId) ??
                   throw new InvalidOperationException(
                       $"Tried to get PeerInputState for non-existent player {playerId}!");
        }

        private void HandlePeerInputMessage(PeerInputMessage message, ulong senderId)
        {
            PeerInputState stateForPlayer = GetOrCreateStateForPlayer(senderId);
            stateForPlayer.netMousePosition = message.netMousePos ?? stateForPlayer.netMousePosition;
            stateForPlayer.isMouseDown = message.mouseDown;
            stateForPlayer.netScreenType = message.screenType;
            // stateForPlayer.isTargeting = message.isTargeting;
            // stateForPlayer.hoveredModelData = message.hoveredModelData;
            // stateForPlayer.isUsingController = message.isUsingController;
            // stateForPlayer.controllerFocusPosition =
            //     message.controllerFocusPosition ?? stateForPlayer.controllerFocusPosition;
            this.StateChanged?.Invoke(senderId);

            NetScreenType netScreenType = stateForPlayer.netScreenType;
            if (netScreenType != stateForPlayer.netScreenType)
            {
                this.ScreenChanged?.Invoke(senderId, netScreenType);
            }
        }

        public Vector2 GetControlSpaceFocusPosition(ulong playerId, RectTransform rootControl)
        {
            PeerInputState peerInputState = ForceGetStateForPlayer(playerId);
            Vector2 vector = (peerInputState.isUsingController
                ? peerInputState.controllerFocusPosition
                : peerInputState.netMousePosition);
            // if (_cursorTranslator == null)
            // {
            return NetCursorHelper.GetControlSpacePosition(vector, rootControl);
            // }
            // Vector2 screenPositionFromNetPosition = _cursorTranslator.GetScreenPositionFromNetPosition(vector);
            // return rootControl.GetGlobalTransformWithCanvas() * screenPositionFromNetPosition;
        }

        public bool GetMouseDown(ulong playerId)
        {
            return ForceGetStateForPlayer(playerId).isMouseDown;
        }

        public NetScreenType GetScreenType(ulong playerId)
        {
            return ForceGetStateForPlayer(playerId).netScreenType;
        }


        public void SyncLocalMousePos(Vector2 mouseScreenPos, RectTransform rootControl)
        {
            PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
            if (_syncMessageToSend == null)
            {
                _syncMessageToSend = new PeerInputMessage();
            }

            Vector2 vector = NetCursorHelper.GetNormalizedPosition(mouseScreenPos, rootControl);
            // Vector2 vector = _cursorTranslator?.GetNetPositionFromScreenPosition(mouseScreenPos) ??
            //                  NetCursorHelper.GetNormalizedPosition(mouseScreenPos, rootControl);
            _syncMessageToSend.netMousePos = vector;
            orCreateStateForPlayer.netMousePosition = vector;
            this.StateChanged?.Invoke(_netService.NetId);
            TrySendSyncMessage();
        }

        public void SyncLocalIsUsingController(bool isUsingController)
        {
            PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
            if (_syncMessageToSend == null)
            {
                _syncMessageToSend = new PeerInputMessage();
            }

            _syncMessageToSend.isUsingController = isUsingController;
            orCreateStateForPlayer.isUsingController = isUsingController;
            this.StateChanged?.Invoke(_netService.NetId);
            TrySendSyncMessage();
        }

        public void SyncLocalMouseDown(bool mouseDown)
        {
            PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
            if (_syncMessageToSend == null)
            {
                _syncMessageToSend = new PeerInputMessage();
            }

            orCreateStateForPlayer.isMouseDown = mouseDown;
            orCreateStateForPlayer.isUsingController = false;
            this.StateChanged?.Invoke(_netService.NetId);
            TrySendSyncMessage();
        }

        public void SyncLocalScreen(NetScreenType netScreenType)
        {
            PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
            if (_syncMessageToSend == null)
            {
                _syncMessageToSend = new PeerInputMessage();
            }

            if (orCreateStateForPlayer.netScreenType != netScreenType)
            {
                //_logger.Debug($"Local screen changed: {orCreateStateForPlayer.netScreenType}->{netScreenType}");
                orCreateStateForPlayer.netScreenType = netScreenType;
                TrySendSyncMessage();
                this.StateChanged?.Invoke(_netService.NetId);
            }
        }

        private void TrySendSyncMessage()
        {
            if (_syncMessageTask == null)
            {
                int num = (int)(_lastSyncSec + 50 - Time.time);
                QueueSyncMessage(num).Forget();
                //
                // int num = (int)(_lastSyncMsec + 50 - Time.time);
                // if (num <= 0)
                // {
                //     SendSyncMessageAfterSmallDelay();
                // }
                // else
                // {
                //     QueueSyncMessage(num);
                // }
            }
        }

        private async UniTask QueueSyncMessage(int delaySec)
        {
            await UniTask.Delay(delaySec);
            SendSyncMessage();
        }

        private void SendSyncMessage()
        {
            if (_syncMessageToSend == null)
            {
                _syncMessageToSend = new PeerInputMessage();
            }

            if (_netService.IsConnected)
            {
                PeerInputState orCreateStateForPlayer = GetOrCreateStateForPlayer(_netService.NetId);
                _syncMessageToSend.mouseDown = orCreateStateForPlayer.isMouseDown;
                _syncMessageToSend.screenType = orCreateStateForPlayer.netScreenType;
                // _syncMessageToSend.isTargeting = orCreateStateForPlayer.isTargeting;
                // _syncMessageToSend.hoveredModelData = orCreateStateForPlayer.hoveredModelData;
                _syncMessageToSend.isUsingController = orCreateStateForPlayer.isUsingController;
                // _syncMessageToSend.controllerFocusPosition = orCreateStateForPlayer.controllerFocusPosition;
                _netService.SendMessage(_syncMessageToSend);
                _lastSyncSec = Time.time;
                _syncMessageToSend = null;
                _syncMessageTask = null;
            }
        }
    }

    public class NetCursorHelper
    {
        public static Vector2 GetNormalizedPosition(Vector2 mouseScreenPos, RectTransform rootNode)
        {
            Vector2 vector = mouseScreenPos;
            //Vector2 vector = rootNode.GetGlobalTransformWithCanvas() * mouseScreenPos;
            Vector2 vector2 = new Vector2(1920f, 1080f);
            return (vector - rootNode.rect.size / 2f) / vector2;
        }

        public static Vector2 GetControlSpacePosition(Vector2 normalizedCursorPosition, RectTransform rootNode)
        {
            Vector2 vector = new Vector2(1920f, 1080f);
            return normalizedCursorPosition * vector + rootNode.rect.size / 2f;
        }
    }
}