using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Models;
using STSLite.Core.Runs;
using System.Collections.Generic;

namespace STSLite.Core.Multiplayer.Game.Lobby
{
    public sealed class NullStartRunLobbyListener : IStartRunLobbyListener
    {
        public void PlayerConnected(LobbyPlayer player)
        {
        }

        public void RemotePlayerDisconnected(LobbyPlayer player)
        {
        }

        public void PlayerChanged(LobbyPlayer player, bool isRandomCharacterResolution)
        {
        }

        public void AscensionChanged()
        {
        }

        public void MaxAscensionChanged()
        {
        }

        public void SeedChanged()
        {
        }

        public void ModifiersChanged()
        {
        }

        public void BeginRun(IReadOnlyList<Player> players, IReadOnlyList<ModifierDefinition> modifiers, string seed, GameMode gameMode)
        {
        }

        public void LocalPlayerDisconnected(NetErrorInfo info)
        {
        }
    }
}
