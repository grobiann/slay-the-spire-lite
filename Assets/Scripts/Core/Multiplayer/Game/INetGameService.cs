using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Multiplayer.Quality;
using STSLite.Core.Multiplayer.Serialization;
using System;

namespace STSLite.Core.Multiplayer.Game
{
    public interface INetGameService
    {
        ulong NetId { get; }
        bool IsConnected { get; }
        bool isGameLoading { get; }
        NetGameType Type { get; }
        PlatformType Platform { get; }
        event Action<NetErrorInfo>? Disconnected;
        void SendMessage<T>(T message, ulong playerId) where T : INetMessage;
        void SendMessage<T>(T message) where T : INetMessage;
        void RegisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage;
        void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage;
        void Update();
        void Disconnect(NetError reason, bool now = false);
        ConnectionStats? GetStatsForPeer(ulong peerId);
        void SetGameLoading(bool loading);
        string? GetRawLobbyIdentifier();
    }
}
