namespace STSLite.Core.Multiplayer.Serialization
{
    public delegate void MessageHandlerDelegate<in T>(T message, ulong senderId) where T : INetMessage;
}
