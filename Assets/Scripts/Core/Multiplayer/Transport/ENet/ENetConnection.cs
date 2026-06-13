using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace STSLite.Core.Multiplayer.Transport.ENet
{
    public sealed class ENetConnection : IDisposable
    {
        public enum EventType
        {
            Error = -1,
            None = 0,
            Connect = 1,
            Disconnect = 2,
            Receive = 3,
        }

        private readonly ConcurrentQueue<ENetServiceData> _events = new ConcurrentQueue<ENetServiceData>();
        private CancellationTokenSource? _cancelSource;
        private TcpListener? _listener;
        private int _maxClients;
        private int _acceptedClients;

        public bool CreateHostBound(string ip, ushort port, int maxClients)
        {
            try
            {
                IPAddress address = IPAddress.Parse(ip);
                _maxClients = maxClients;
                _cancelSource = new CancellationTokenSource();
                _listener = new TcpListener(address, port);
                _listener.Start(maxClients);
                _ = AcceptLoop(_cancelSource.Token);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start ENet-compatible host on {ip}:{port}: {ex}");
                return false;
            }
        }

        public bool TryService(out ENetServiceData data)
        {
            return _events.TryDequeue(out data);
        }

        public void Flush()
        {
        }

        public void Destroy()
        {
            Dispose();
        }

        private async Task AcceptLoop(CancellationToken cancelToken)
        {
            if (_listener == null)
            {
                return;
            }

            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    if (_acceptedClients >= _maxClients)
                    {
                        client.Dispose();
                        continue;
                    }

                    _acceptedClients++;
                    ENetPacketPeer peer = new ENetPacketPeer(client);
                    _events.Enqueue(new ENetServiceData(EventType.Connect, peer));
                    _ = ReceiveLoop(peer, cancelToken);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"ENet-compatible host accept failed: {ex}");
                    _events.Enqueue(new ENetServiceData(EventType.Error, null));
                }
            }
        }

        private Task ReceiveLoop(ENetPacketPeer peer, CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    while (!cancelToken.IsCancellationRequested && peer.IsActive)
                    {
                        if (!peer.TryReadPacket(out string json, out NetTransferMode mode, out int channel))
                        {
                            break;
                        }

                        _events.Enqueue(new ENetServiceData(EventType.Receive, peer, json, mode, channel));
                    }
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception ex)
                {
                    Debug.LogError($"ENet-compatible host receive failed: {ex}");
                    _events.Enqueue(new ENetServiceData(EventType.Error, peer));
                }
                finally
                {
                    _events.Enqueue(new ENetServiceData(EventType.Disconnect, peer));
                }
            }, cancelToken);
        }

        public void Dispose()
        {
            _cancelSource?.Cancel();
            _listener?.Stop();
            _cancelSource?.Dispose();
            _cancelSource = null;
            _listener = null;
        }
    }
}