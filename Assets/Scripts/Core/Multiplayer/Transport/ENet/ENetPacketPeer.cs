using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace STSLite.Core.Multiplayer.Transport.ENet
{
    public sealed class ENetPacketPeer : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly object _sendLock = new object();
        private bool _disposed;

        public ENetPacketPeer(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public bool IsActive => !_disposed && _client.Connected;

        public void Send(int channel, string json, NetTransferMode mode)
        {
            if (_disposed)
            {
                return;
            }

            byte[] payload = Encoding.UTF8.GetBytes(json);
            byte[] header = new byte[12];
            BitConverter.GetBytes(payload.Length).CopyTo(header, 0);
            BitConverter.GetBytes((int)mode).CopyTo(header, 4);
            BitConverter.GetBytes(channel).CopyTo(header, 8);

            lock (_sendLock)
            {
                _stream.Write(header, 0, header.Length);
                _stream.Write(payload, 0, payload.Length);
                _stream.Flush();
            }
        }

        public bool TryReadPacket(out string json, out NetTransferMode mode, out int channel)
        {
            json = "";
            mode = NetTransferMode.Reliable;
            channel = 0;

            byte[] header = ReadExact(12);
            if (header.Length == 0)
            {
                return false;
            }

            int length = BitConverter.ToInt32(header, 0);
            mode = (NetTransferMode)BitConverter.ToInt32(header, 4);
            channel = BitConverter.ToInt32(header, 8);

            if (length < 0)
            {
                return false;
            }

            byte[] payload = ReadExact(length);
            if (payload.Length != length)
            {
                return false;
            }

            json = Encoding.UTF8.GetString(payload);
            return true;
        }

        private byte[] ReadExact(int length)
        {
            byte[] buffer = new byte[length];
            int offset = 0;

            while (offset < length)
            {
                int read = _stream.Read(buffer, offset, length - offset);
                if (read <= 0)
                {
                    return Array.Empty<byte>();
                }

                offset += read;
            }

            return buffer;
        }

        public void PeerDisconnect()
        {
            Dispose();
        }

        public void PeerDisconnectNow()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _stream.Dispose();
            _client.Dispose();
        }
    }
}
