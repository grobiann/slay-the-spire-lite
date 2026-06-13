using STSLite.Core.Entities.Multiplayer;
using System.Collections.Generic;

namespace STSLite.Core.Multiplayer.Transport
{
    public abstract class NetHost
    {
        protected INetHostHandler _handler;
        public abstract IEnumerable<ulong> ConnectedPeerIds { get; }
        public abstract bool IsConnected { get; }
        public abstract ulong NetId { get; }

        protected NetHost(INetHostHandler handler)
        {
            _handler = handler;
        }

        public abstract void Update();
        public abstract void SetHostIsClosed(bool isClosded);

        public abstract void SendMessageToClient(ulong peerId, string json, int length, NetTransferMode mode,
            int channel = 0);

        public abstract void SendMessageToAll(string json, int length, NetTransferMode mode, int channel = 0);
        public abstract void DisconnectClient(ulong peerId, NetError reason, bool now = false);
        public abstract void StopHost(NetError reason, bool now = false);
        public abstract string? GetRawLobbyIdentifier();
    }
}