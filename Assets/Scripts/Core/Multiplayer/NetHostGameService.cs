using NUnit.Framework;
using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Multiplayer.Game;
using STSLite.Core.Multiplayer.Quality;
using STSLite.Core.Multiplayer.Serialization;
using STSLite.Core.Multiplayer.Transport;
using STSLite.Core.Multiplayer.Transport.ENet;
using System;
using System.Collections.Generic;

namespace STSLite.Core.Multiplayer
{
    public class NetHostGameService : INetGameService, INetHostHandler
    {
        private NetHost? _netHost;
        private readonly NetMessageBus _messageBus = new NetMessageBus();
        //private readonly NetQualityTracker _qualityTracker;
        private readonly List<NetClientData> _connectedPeers = new List<NetClientData>();

        public bool IsConnected => _netHost?.IsConnected ?? false;
        public IReadOnlyList<NetClientData> ConnectedPeers => _connectedPeers;
        public ulong NetId => (_netHost ?? throw new InvalidOperationException("NetHost is not initialized")).NetId;
        public bool isGameLoading { get; private set; }
        public PlatformType Platform { get; private set; }
        public NetHost? NetHost => _netHost;
        public NetGameType Type => NetGameType.Host;
        public event Action<NetErrorInfo>? Disconnected;
        public event Action<ulong>? ClientConnected;
        public event Action<ulong, NetErrorInfo>? ClientDisconnected;

        public NetHostGameService()
        {
            // _qualityTracker = new NetQualityTracker(this);
        }

        public NetErrorInfo? StartENetHost(ushort port, int maxClients)
        {
            return ((ENetHost)(_netHost = new ENetHost(this))).StartHost(port, maxClients);
        }

        public void Update()
        {
            _netHost?.Update();
        }

        public void Disconnect(NetError reason, bool now = false)
        {
            _netHost?.StopHost(reason, now);
        }

        public void DisconnectClient(ulong peerId, NetError reason, bool now = false)
        {
            _netHost?.DisconnectClient(peerId, reason, now);
        }

        public void SetPeerReadyForBroadcasting(ulong peerId)
        {
            int index = _connectedPeers.FindIndex(peer => peer.peerId == peerId);
            if (index < 0)
            {
                return;
            }

            NetClientData clientData = _connectedPeers[index];
            clientData.readyForBroadcasting = true;
            _connectedPeers[index] = clientData;
        }

        public void SendMessage<T>(T message, ulong playerId) where T : INetMessage
        {
            if (_netHost == null)
            {
                return;
            }

            string json = _messageBus.Serialize(message);
            _netHost.SendMessageToClient(playerId, json, json.Length, NetTransferMode.Reliable);
        }

        public void SendMessage<T>(T message) where T : INetMessage
        {
            if (_netHost == null)
            {
                return;
            }

            string json = _messageBus.Serialize(message);
            foreach (NetClientData peer in _connectedPeers)
            {
                if (!peer.readyForBroadcasting)
                {
                    continue;
                }

                _netHost.SendMessageToClient(peer.peerId, json, json.Length, NetTransferMode.Reliable);
            }
        }

        public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage
        {
            _messageBus.RegisterMessageHandler(messageHandlerDelegate);
        }

        public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage
        {
            _messageBus.UnregisterMessageHandler(messageHandlerDelegate);
        }

        public ConnectionStats? GetStatsForPeer(ulong peerId)
        {
            return null;
        }

        public void SetGameLoading(bool loading)
        {
            isGameLoading = loading;
        }

        public string? GetRawLobbyIdentifier()
        {
            return _netHost?.GetRawLobbyIdentifier();
        }

        public void OnPacketReceived(ulong senderId, string packetJson, NetTransferMode mode, int channel)
        {
            _messageBus.Dispatch(packetJson, senderId);
        }

        public void OnPeerConnected(ulong peerId)
        {
            _connectedPeers.Add(new NetClientData
            {
                peerId = peerId,
                readyForBroadcasting = false,
            });
            ClientConnected?.Invoke(peerId);
        }

        public void OnPeerDisconnected(ulong peerId, NetErrorInfo reason)
        {
            _connectedPeers.RemoveAll(peer => peer.peerId == peerId);
            ClientDisconnected?.Invoke(peerId, reason);
        }

        public void OnDisconnected(NetErrorInfo reason)
        {
            Disconnected?.Invoke(reason);
        }

    }
}
