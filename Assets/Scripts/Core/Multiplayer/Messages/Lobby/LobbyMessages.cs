using STSLite.Core.Models;
using STSLite.Core.Multiplayer.Game.Lobby;
using STSLite.Core.Multiplayer.Serialization;
using System;
using System.Collections.Generic;

namespace STSLite.Core.Multiplayer.Messages.Lobby
{
    [Serializable]
    public class ClientLobbyJoinRequestMessage : INetMessage
    {
        public string characterId = string.Empty;
        public int maxAscensionUnlocked;
        public bool ShouldBroadcast => false;
    }

    [Serializable]
    public class ClientLobbyJoinResponseMessage : INetMessage
    {
        public ulong localPlayerId;
        public List<LobbyPlayer> playersInLobby = new List<LobbyPlayer>();
        public int ascension;
        public int maxAscension;
        public string seed = string.Empty;
        public List<ModifierDefinition> modifiers = new List<ModifierDefinition>();
        public bool ShouldBroadcast => false;
    }

    [Serializable]
    public class PlayerJoinedMessage : INetMessage
    {
        public LobbyPlayer lobbyPlayer;
        public bool ShouldBroadcast => true;
    }

    [Serializable]
    public class PlayerLeftMessage : INetMessage
    {
        public ulong playerId;
        public bool ShouldBroadcast => true;
    }

    [Serializable]
    public class LobbyPlayerChangedCharacterMessage : INetMessage
    {
        public CharacterDefinition character;
        public bool ShouldBroadcast => true;
    }

    [Serializable]
    public class LobbyAscensionChangedMessage : INetMessage
    {
        public int ascension;
        public bool ShouldBroadcast => true;
    }

    [Serializable]
    public class LobbySeedChangedMessage : INetMessage
    {
        public string seed = string.Empty;
        public bool ShouldBroadcast => true;
    }

    [Serializable]
    public class LobbyModifiersChangedMessage : INetMessage
    {
        public List<ModifierDefinition> modifiers = new List<ModifierDefinition>();
        public bool ShouldBroadcast => true;
    }

    [Serializable]
    public class LobbyPlayerSetReadyMessage : INetMessage
    {
        public bool ready;
        public bool ShouldBroadcast => true;
    }

    [Serializable]
    public class LobbyBeginRunMessage : INetMessage
    {
        public List<LobbyPlayer> playersInLobby = new List<LobbyPlayer>();
        public string seed = string.Empty;
        public List<ModifierDefinition> modifiers = new List<ModifierDefinition>();
        public bool ShouldBroadcast => true;
    }
}