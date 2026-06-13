namespace STSLite.Core.Multiplayer.Game
{
    public static class NetGameTypeExtensions
    {
        public static bool IsMultiplayer(this NetGameType gameType)
        {
            return gameType != NetGameType.Singleplayer;
        }
    }
}