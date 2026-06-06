namespace STSLite.Core.Multiplayer.Transport.ENet
{
    public readonly struct ENetServiceData
    {
        public readonly ENetConnection.EventType type;
        public readonly ENetPacketPeer? peer;
        public readonly string packetJson;
        public readonly NetTransferMode mode;
        public readonly int channel;

        public ENetServiceData(
            ENetConnection.EventType type,
            ENetPacketPeer? peer,
            string packetJson = "",
            NetTransferMode mode = NetTransferMode.Reliable,
            int channel = 0)
        {
            this.type = type;
            this.peer = peer;
            this.packetJson = packetJson;
            this.mode = mode;
            this.channel = channel;
        }
    }
}
