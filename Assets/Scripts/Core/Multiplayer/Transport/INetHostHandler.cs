using STSLite.Core.Entities.Multiplayer;

namespace STSLite.Core.Multiplayer.Transport
{
    public interface INetHostHandler : INetHandler
    {
        void OnPeerConnected(ulong peerId);
        void OnPeerDisconnected(ulong peerId, NetErrorInfo reason);
        void OnDisconnected(NetErrorInfo reason);
    }
}