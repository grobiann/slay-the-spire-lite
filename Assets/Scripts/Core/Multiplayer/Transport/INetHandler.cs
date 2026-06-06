namespace STSLite.Core.Multiplayer.Transport
{
    public interface INetHandler
    {
        void OnPacketReceived(ulong senderId, string packetJson, NetTransferMode mode, int channel);
    }
}
