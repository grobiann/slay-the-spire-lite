using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Multiplayer.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace STSLite.Core.Multiplayer.Transport.ENet
{
    public class ENetHost : NetHost
    {
        private struct ClientConnection
        {
            public ulong netId;
            public ENetPacketPeer peer;
        }

        //private struct HandshakeAwaitingResponse
        //{
        //    public ulong receivedMsec;
        //    public ClientConnection conn;
        //}

        private const int _handshakeTimeoutMsec = 10000;
        private const int _handshakeUpdateRateMsec = 100;

        private readonly List<ClientConnection> _connectedPeers = new List<ClientConnection>();

        private ENetConnection? _connection;
        private bool _isConnected;
        public override bool IsConnected => _isConnected;

        private ulong _nextPeerId = 1000;
        public override IEnumerable<ulong> ConnectedPeerIds => _connectedPeers.Select(peer => peer.netId);
        public override ulong NetId => 1uL;

        public ENetHost(INetHostHandler handler) : base(handler)
        {
        }

        public NetErrorInfo? StartHost(ushort port, int maxClients)
        {
            _connection = new ENetConnection();
            if (!_connection.CreateHostBound("0.0.0.0", port, maxClients))
            {
                return new NetErrorInfo(NetError.FailedToHost, selfInitiated: true);
            }

            _isConnected = true;
            return null;
        }

        public override void Update()
        {
            if (_connection == null)
            {
                return;
            }

            while (_connection.TryService(out ENetServiceData data))
            {
                switch (data.type)
                {
                    case ENetConnection.EventType.Connect:
                        HandlePeerConnected(data.peer);
                        break;
                    case ENetConnection.EventType.Receive:
                        HandlePacketReceived(data);
                        break;
                    case ENetConnection.EventType.Disconnect:
                        HandlePeerDisconnected(data.peer, NetError.UnknownNetworkError, notifyHandler: true);
                        break;
                    case ENetConnection.EventType.Error:
                        Debug.LogError("ENet-compatible host received a transport error.");
                        break;
                    case ENetConnection.EventType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void SetHostIsClosed(bool isClosded)
        {
        }

        public override void SendMessageToClient(ulong peerId, string json, int length, NetTransferMode mode,
            int channel = 0)
        {
            ClientConnection? connection = GetConnectionById(peerId);
            if (!connection.HasValue)
            {
                Debug.LogError($"Tried to send message to client {peerId}, but no client with that ID is connected.");
                return;
            }

            connection.Value.peer.Send(channel, json, mode);
        }

        public override void SendMessageToAll(string json, int length, NetTransferMode mode, int channel = 0)
        {
            foreach (ClientConnection connection in _connectedPeers.ToList())
            {
                connection.peer.Send(channel, json, mode);
            }
        }

        public override void DisconnectClient(ulong peerId, NetError reason, bool now = false)
        {
            ClientConnection? connection = GetConnectionById(peerId);
            if (!connection.HasValue)
            {
                return;
            }

            if (now)
            {
                connection.Value.peer.PeerDisconnectNow();
            }
            else
            {
                connection.Value.peer.PeerDisconnect();
            }

            HandlePeerDisconnected(connection.Value.peer, reason, notifyHandler: true);
        }

        public override void StopHost(NetError reason, bool now = false)
        {
            foreach (ClientConnection connection in _connectedPeers.ToList())
            {
                if (now)
                {
                    connection.peer.PeerDisconnectNow();
                }
                else
                {
                    connection.peer.PeerDisconnect();
                }
            }

            _connectedPeers.Clear();
            _connection?.Destroy();
            _connection = null;
            _isConnected = false;
            _handler.OnDisconnected(new NetErrorInfo(reason, selfInitiated: true));
        }

        public override string? GetRawLobbyIdentifier()
        {
            return null;
        }

        private void HandlePeerConnected(ENetPacketPeer? peer)
        {
            if (peer == null)
            {
                return;
            }

            ulong netId = _nextPeerId++;
            _connectedPeers.Add(new ClientConnection
            {
                netId = netId,
                peer = peer,
            });
            _handler.OnPeerConnected(netId);
        }

        private void HandlePacketReceived(ENetServiceData data)
        {
            if (data.peer == null)
            {
                return;
            }

            ClientConnection? connection = GetConnectionByPeer(data.peer);
            if (!connection.HasValue)
            {
                Debug.LogError("Received a packet from a peer that is not registered.");
                return;
            }

            _handler.OnPacketReceived(connection.Value.netId, data.packetJson, data.mode, data.channel);
        }

        private void HandlePeerDisconnected(ENetPacketPeer? peer, NetError reason, bool notifyHandler)
        {
            if (peer == null)
            {
                return;
            }

            ClientConnection? connection = GetConnectionByPeer(peer);
            if (!connection.HasValue)
            {
                return;
            }

            _connectedPeers.Remove(connection.Value);
            peer.Dispose();

            if (notifyHandler)
            {
                _handler.OnPeerDisconnected(connection.Value.netId, new NetErrorInfo(reason, selfInitiated: false));
            }
        }

        private ClientConnection? GetConnectionByPeer(ENetPacketPeer peer)
        {
            foreach (ClientConnection connection in _connectedPeers)
            {
                if (ReferenceEquals(connection.peer, peer))
                {
                    return connection;
                }
            }

            return null;
        }

        private ClientConnection? GetConnectionById(ulong peerId)
        {
            foreach (ClientConnection connection in _connectedPeers)
            {
                if (connection.netId == peerId)
                {
                    return connection;
                }
            }

            return null;
        }
    }
}