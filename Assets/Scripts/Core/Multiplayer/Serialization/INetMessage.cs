namespace STSLite.Core.Multiplayer.Serialization
{
    public interface INetMessage
    {
        bool ShouldBroadcast { get; }
    }
}
