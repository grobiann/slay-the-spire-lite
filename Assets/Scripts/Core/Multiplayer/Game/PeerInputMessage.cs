using STSLite.Core.Multiplayer.Serialization;

namespace STSLite.Core.Multiplayer.Game
{
    [System.Serializable]
    public class PeerInputMessage : INetMessage
    {
        public bool mouseDown;
        public SerializableVector2? netMousePos;
        public NetScreenType screenType;
        public bool isUsingController;

        public bool ShouldBroadcast => true;
        //public NetTransferMode Mode => NetTransferMode.Unreliable;
    }
}