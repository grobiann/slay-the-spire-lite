namespace STSLite.Core.Entities.Multiplayer
{
    public readonly struct NetErrorInfo
    {
        public NetError Reason { get; }
        public string? DebugReason { get; }
        public bool SelfInitiated { get; }

        public NetErrorInfo(NetError reason, string? debugReason = null, bool selfInitiated = false)
        {
            Reason = reason;
            DebugReason = debugReason;
            SelfInitiated = selfInitiated;
        }
    }
}