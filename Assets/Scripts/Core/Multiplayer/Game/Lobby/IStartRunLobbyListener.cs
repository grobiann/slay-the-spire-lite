using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Models;
using STSLite.Core.Runs;
using System.Collections.Generic;

namespace STSLite.Core.Multiplayer.Game.Lobby
{
    public interface IStartRunLobbyListener
    {
        void PlayerConnected(LobbyPlayer player);
        void RemotePlayerDisconnected(LobbyPlayer player);
        void PlayerChanged(LobbyPlayer player, bool isRandomCharacterResolution);
        void AscensionChanged();
        void MaxAscensionChanged();
        void SeedChanged();
        void ModifiersChanged();
        void BeginRun(IReadOnlyList<Player> players, IReadOnlyList<ModifierDefinition> modifiers, string seed, GameMode gameMode);
        void LocalPlayerDisconnected(NetErrorInfo info);
    }
}
