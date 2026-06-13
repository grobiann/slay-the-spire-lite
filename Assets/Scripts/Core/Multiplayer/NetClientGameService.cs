using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Multiplayer.Game;
using STSLite.Core.Multiplayer.Messages.Lobby;
using STSLite.Core.Multiplayer.Quality;
using STSLite.Core.Multiplayer.Serialization;
using STSLite.Core.Multiplayer.Transport;
using STSLite.Core.Multiplayer.Transport.ENet;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace STSLite.Core.Multiplayer
{
    public class NetClientGameService : INetGameService
    {
        private readonly NetMessageBus _messageBus = new NetMessageBus();
        private readonly ConcurrentQueue<ClientPacket> _packets = new ConcurrentQueue<ClientPacket>();

        private CancellationTokenSource _cancelSource;
        private ENetPacketPeer _peer;
        private bool _isConnected;
        private ulong _netId = 0uL;

        public ulong NetId => _netId;
        public bool IsConnected => _isConnected;
        public bool isGameLoading { get; private set; }
        public NetGameType Type => NetGameType.Client;
        public PlatformType Platform => PlatformType.None;

        public event Action<NetErrorInfo> Disconnected;

        public NetErrorInfo? StartENetClient(string host, ushort port)
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(host, port);
                _peer = new ENetPacketPeer(tcpClient);
                _cancelSource = new CancellationTokenSource();
                _isConnected = true;
                _messageBus.RegisterMessageHandler<ClientLobbyJoinResponseMessage>(
                    HandleClientLobbyJoinResponseMessage);
                _ = ReceiveLoop(_cancelSource.Token);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to connect to host {host}:{port}: {ex}");
                return new NetErrorInfo(NetError.InvalidJoin, ex.Message, selfInitiated: true);
            }
        }

        public void SendMessage<T>(T message, ulong playerId) where T : INetMessage
        {
            SendMessage(message);
        }

        public void SendMessage<T>(T message) where T : INetMessage
        {
            if (_peer == null || !_peer.IsActive)
            {
                return;
            }

            string json = _messageBus.Serialize(message);
            _peer.Send(0, json, NetTransferMode.Reliable);
        }

        public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage
        {
            _messageBus.RegisterMessageHandler(messageHandlerDelegate);
        }

        public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage
        {
            _messageBus.UnregisterMessageHandler(messageHandlerDelegate);
        }

        public void Update()
        {
            while (_packets.TryDequeue(out ClientPacket packet))
            {
                _messageBus.Dispatch(packet.json, 1uL);
            }
        }

        public void Disconnect(NetError reason, bool now = false)
        {
            if (!_isConnected)
            {
                return;
            }

            _isConnected = false;
            _cancelSource?.Cancel();
            _peer?.Dispose();
            Disconnected?.Invoke(new NetErrorInfo(reason, selfInitiated: true));
        }

        public ConnectionStats GetStatsForPeer(ulong peerId)
        {
            return null;
        }

        public void SetGameLoading(bool loading)
        {
            isGameLoading = loading;
        }

        public string GetRawLobbyIdentifier()
        {
            return null;
        }

        private Task ReceiveLoop(CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    while (!cancelToken.IsCancellationRequested && _peer != null && _peer.IsActive)
                    {
                        if (!_peer.TryReadPacket(out string json, out NetTransferMode mode, out int channel))
                        {
                            break;
                        }

                        ClientPacket packet = new ClientPacket();
                        packet.json = json;
                        _packets.Enqueue(packet);
                    }
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Client receive loop failed: {ex}");
                }
                finally
                {
                    if (_isConnected)
                    {
                        _isConnected = false;
                        Disconnected?.Invoke(new NetErrorInfo(NetError.UnknownNetworkError, selfInitiated: false));
                    }
                }
            }, cancelToken);
        }

        private void HandleClientLobbyJoinResponseMessage(ClientLobbyJoinResponseMessage message, ulong senderId)
        {
            _netId = message.localPlayerId;
        }

        private struct ClientPacket
        {
            public string json;
        }
    }
}