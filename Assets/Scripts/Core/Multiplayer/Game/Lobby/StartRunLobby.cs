using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Models;
using STSLite.Core.Multiplayer.Messages.Lobby;
using STSLite.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = STSLite.Core.Logging.Logger;
using LogType = STSLite.Core.Logging.LogType;

namespace STSLite.Core.Multiplayer.Game.Lobby
{
    public class StartRunLobby
    {
        private readonly Logger _logger;
        private readonly List<ModifierDefinition> _modifiers = new List<ModifierDefinition>();

        private bool _isBeginningRun;

        public INetGameService NetService { get; }
        public IStartRunLobbyListener LobbyListener { get; }
        public PeerInputSynchronizer InputSynchronizer { get; }
        public int MaxPlayers { get; private set; }
        public int Ascension { get; private set; }
        public int MaxAscension { get; private set; }
        public string Seed { get; private set; }
        public GameMode GameMode { get; private set; }
        public IReadOnlyList<ModifierDefinition> Modifiers => _modifiers;
        public List<LobbyPlayer> Players { get; } = new List<LobbyPlayer>();
        public LobbyPlayer LocalPlayer => GetPlayer(NetService.NetId);

        public event Action<LobbyPlayer> PlayerConnected;
        public event Action<LobbyPlayer> PlayerDisconnected;
        public event Action<IReadOnlyList<Player>, IReadOnlyList<ModifierDefinition>, string, GameMode> BeginRun;

        public StartRunLobby(GameMode gameMode, INetGameService netService, IStartRunLobbyListener lobbyListener,
            int maxPlayers)
        {
            GameMode = gameMode;
            NetService = netService;
            LobbyListener = lobbyListener;
            InputSynchronizer = new PeerInputSynchronizer(netService);
            MaxPlayers = maxPlayers;
            Seed = string.Empty;
            _logger = new Logger("StartRunLobby", LogType.Network);
            _logger.Context = $"StartRunLobby ({NetService.NetId})";

            NetService.RegisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
            NetService.RegisterMessageHandler<ClientLobbyJoinResponseMessage>(HandleClientLobbyJoinResponseMessage);
            NetService.RegisterMessageHandler<PlayerJoinedMessage>(HandlePlayerJoinedMessage);
            NetService.RegisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
            NetService.RegisterMessageHandler<LobbyPlayerChangedCharacterMessage>(
                HandleLobbyPlayerChangedCharacterMessage);
            NetService.RegisterMessageHandler<LobbyAscensionChangedMessage>(HandleAscensionChangedMessage);
            NetService.RegisterMessageHandler<LobbySeedChangedMessage>(HandleSeedChangedMessage);
            NetService.RegisterMessageHandler<LobbyModifiersChangedMessage>(HandleModifiersChangedMessage);
            NetService.RegisterMessageHandler<LobbyPlayerSetReadyMessage>(HandlePlayerReadyMessage);
            NetService.RegisterMessageHandler<LobbyBeginRunMessage>(HandleLobbyBeginRunMessage);
            NetService.Disconnected += OnDisconnected;

            NetHostGameService hostGameService = NetService as NetHostGameService;
            if (hostGameService != null)
            {
                hostGameService.ClientConnected += OnConnectedToClientAsHost;
                hostGameService.ClientDisconnected += OnDisconnectedFromClientAsHost;

                foreach (NetClientData connectedPeer in hostGameService.ConnectedPeers)
                {
                    OnConnectedToClientAsHost(connectedPeer.peerId);
                }
            }
        }

        public StartRunLobby(GameMode gameMode, INetGameService netService, int maxPlayers)
            : this(gameMode, netService, new NullStartRunLobbyListener(), maxPlayers)
        {
        }

        public void InitializeFromMessage(ClientLobbyJoinResponseMessage message)
        {
            Players.Clear();
            Players.AddRange(message.playersInLobby);

            _modifiers.Clear();
            _modifiers.AddRange(message.modifiers);

            Ascension = message.ascension;
            MaxAscension = message.maxAscension;
            Seed = message.seed ?? string.Empty;

            LobbyPlayer localPlayer = LocalPlayer;
            LobbyListener.PlayerConnected(localPlayer);
            PlayerConnected?.Invoke(localPlayer);
        }

        public void CleanUp(bool disconnectSession)
        {
            NetService.UnregisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
            NetService.UnregisterMessageHandler<ClientLobbyJoinResponseMessage>(HandleClientLobbyJoinResponseMessage);
            NetService.UnregisterMessageHandler<PlayerJoinedMessage>(HandlePlayerJoinedMessage);
            NetService.UnregisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
            NetService.UnregisterMessageHandler<LobbyPlayerChangedCharacterMessage>(
                HandleLobbyPlayerChangedCharacterMessage);
            NetService.UnregisterMessageHandler<LobbyAscensionChangedMessage>(HandleAscensionChangedMessage);
            NetService.UnregisterMessageHandler<LobbySeedChangedMessage>(HandleSeedChangedMessage);
            NetService.UnregisterMessageHandler<LobbyModifiersChangedMessage>(HandleModifiersChangedMessage);
            NetService.UnregisterMessageHandler<LobbyPlayerSetReadyMessage>(HandlePlayerReadyMessage);
            NetService.UnregisterMessageHandler<LobbyBeginRunMessage>(HandleLobbyBeginRunMessage);
            NetService.Disconnected -= OnDisconnected;

            NetHostGameService hostGameService = NetService as NetHostGameService;
            if (hostGameService != null)
            {
                hostGameService.ClientConnected -= OnConnectedToClientAsHost;
                hostGameService.ClientDisconnected -= OnDisconnectedFromClientAsHost;
            }

            if (disconnectSession && NetService.IsConnected)
            {
                NetService.Disconnect(NetError.Quit);
            }
        }

        public LobbyPlayer AddLocalHostPlayer()
        {
            if (NetService.Type == NetGameType.Client)
            {
                throw new InvalidOperationException("Client cannot add the host player.");
            }

            LobbyPlayer lobbyPlayer = AddPlayerInFirstAvailableSlot(NetService.NetId);
            NotifyPlayerConnected(lobbyPlayer);
            return lobbyPlayer;
        }

        public void SetLocalCharacter(CharacterDefinition character)
        {
            ChangeCharacter(NetService.NetId, character, false);

            LobbyPlayerChangedCharacterMessage message = new LobbyPlayerChangedCharacterMessage();
            message.character = character;
            NetService.SendMessage(message);
        }

        public void SetSeed(string seed)
        {
            EnsureHostOrSingleplayer();

            Seed = seed ?? string.Empty;

            LobbySeedChangedMessage message = new LobbySeedChangedMessage();
            message.seed = Seed;
            NetService.SendMessage(message);

            LobbyListener.SeedChanged();
        }

        public void SetModifiers(IReadOnlyList<ModifierDefinition> modifiers)
        {
            EnsureHostOrSingleplayer();

            _modifiers.Clear();
            _modifiers.AddRange(modifiers);

            LobbyModifiersChangedMessage message = new LobbyModifiersChangedMessage();
            message.modifiers = new List<ModifierDefinition>(_modifiers);
            NetService.SendMessage(message);

            LobbyListener.ModifiersChanged();
        }

        public void SetReady(bool ready)
        {
            LobbyPlayer lobbyPlayer = LocalPlayer;
            lobbyPlayer.isReady = ready;

            LobbyPlayerSetReadyMessage message = new LobbyPlayerSetReadyMessage();
            message.ready = ready;
            NetService.SendMessage(message);

            LobbyListener.PlayerChanged(lobbyPlayer, false);
            _logger.Info($"Local player {lobbyPlayer.id} ready: {ready}");

            BeginRunIfAllPlayersReady();
        }

        public void SyncAscensionChange(int ascension)
        {
            EnsureHostOrSingleplayer();

            if (Ascension == ascension)
            {
                return;
            }

            Ascension = ascension;

            LobbyAscensionChangedMessage message = new LobbyAscensionChangedMessage();
            message.ascension = ascension;
            NetService.SendMessage(message);

            LobbyListener.AscensionChanged();
        }

        public bool IsAboutToBeginGame()
        {
            if (_isBeginningRun)
            {
                return false;
            }

            if (Players.Count == 0)
            {
                return false;
            }

            if (NetService.Type == NetGameType.Host && Players.Count < 2)
            {
                return false;
            }

            return Players.All(player => player.isReady);
        }

        public void BeginRunIfAllPlayersReady()
        {
            if (!IsAboutToBeginGame())
            {
                return;
            }

            EnsureHostOrSingleplayer();

            string runSeed = string.IsNullOrWhiteSpace(Seed) ? CreateRandomSeed() : Seed.Trim();
            BeginRunForAllPlayers(runSeed, _modifiers);
        }

        public void BeginRunForAllPlayers(string seed, IReadOnlyList<ModifierDefinition> modifiers)
        {
            EnsureHostOrSingleplayer();

            if (_isBeginningRun)
            {
                _logger.Warn("Tried to begin the lobby run twice.");
                return;
            }

            LobbyBeginRunMessage message = new LobbyBeginRunMessage();
            message.playersInLobby = new List<LobbyPlayer>(Players);
            message.seed = seed;
            message.modifiers = new List<ModifierDefinition>(modifiers);
            NetService.SendMessage(message);

            BeginRunLocally(seed, modifiers);
        }

        private void HandleClientLobbyJoinRequestMessage(ClientLobbyJoinRequestMessage message, ulong senderId)
        {
            if (NetService.Type != NetGameType.Host)
            {
                return;
            }

            LobbyPlayer lobbyPlayer = TryGetPlayerOrNull(senderId);
            if (lobbyPlayer == null)
            {
                if (Players.Count >= MaxPlayers)
                {
                    DisconnectClient(senderId, NetError.LobbyFull);
                    return;
                }

                lobbyPlayer = AddPlayerInFirstAvailableSlot(senderId);
            }

            lobbyPlayer.character = GetCharacterOrDefault(message.characterId);

            ClientLobbyJoinResponseMessage response = new ClientLobbyJoinResponseMessage();
            response.localPlayerId = senderId;
            response.playersInLobby = new List<LobbyPlayer>(Players);
            response.ascension = Ascension;
            response.maxAscension = MaxAscension;
            response.seed = Seed;
            response.modifiers = new List<ModifierDefinition>(_modifiers);
            NetService.SendMessage(response, senderId);

            NetHostGameService hostGameService = NetService as NetHostGameService;
            if (hostGameService != null)
            {
                hostGameService.SetPeerReadyForBroadcasting(senderId);
            }

            NotifyPlayerConnected(lobbyPlayer);
            BroadcastPlayerJoined(lobbyPlayer, senderId);
        }

        private void HandleClientLobbyJoinResponseMessage(ClientLobbyJoinResponseMessage message, ulong senderId)
        {
            InitializeFromMessage(message);
        }

        private void HandlePlayerJoinedMessage(PlayerJoinedMessage message, ulong senderId)
        {
            if (TryGetPlayerOrNull(message.lobbyPlayer.id) != null)
            {
                return;
            }

            Players.Add(message.lobbyPlayer);
            NotifyPlayerConnected(message.lobbyPlayer);
        }

        private void HandlePlayerLeftMessage(PlayerLeftMessage message, ulong senderId)
        {
            RemovePlayer(message.playerId);
        }

        private void HandleLobbyPlayerChangedCharacterMessage(LobbyPlayerChangedCharacterMessage message,
            ulong senderId)
        {
            ChangeCharacter(senderId, message.character, false);
        }

        private void HandleAscensionChangedMessage(LobbyAscensionChangedMessage message, ulong senderId)
        {
            Ascension = message.ascension;
            LobbyListener.AscensionChanged();
        }

        private void HandleSeedChangedMessage(LobbySeedChangedMessage message, ulong senderId)
        {
            Seed = message.seed ?? string.Empty;
            LobbyListener.SeedChanged();
        }

        private void HandleModifiersChangedMessage(LobbyModifiersChangedMessage message, ulong senderId)
        {
            _modifiers.Clear();
            _modifiers.AddRange(message.modifiers);
            LobbyListener.ModifiersChanged();
        }

        private void HandlePlayerReadyMessage(LobbyPlayerSetReadyMessage message, ulong senderId)
        {
            LobbyPlayer lobbyPlayer = TryGetPlayerOrNull(senderId);
            if (lobbyPlayer == null)
            {
                return;
            }

            lobbyPlayer.isReady = message.ready;
            LobbyListener.PlayerChanged(lobbyPlayer, false);
            BeginRunIfAllPlayersReady();
        }

        private void HandleLobbyBeginRunMessage(LobbyBeginRunMessage message, ulong senderId)
        {
            Players.Clear();
            Players.AddRange(message.playersInLobby);

            BeginRunLocally(message.seed, message.modifiers);
        }

        private void OnConnectedToClientAsHost(ulong playerId)
        {
            if (_isBeginningRun)
            {
                DisconnectClient(playerId, NetError.RunInProgress);
                return;
            }

            if (Players.Count >= MaxPlayers)
            {
                DisconnectClient(playerId, NetError.LobbyFull);
                return;
            }

            if (TryGetPlayerOrNull(playerId) != null)
            {
                return;
            }

            LobbyPlayer lobbyPlayer = AddPlayerInFirstAvailableSlot(playerId);
            NotifyPlayerConnected(lobbyPlayer);
            BroadcastPlayerJoined(lobbyPlayer, playerId);

            ClientLobbyJoinResponseMessage response = new ClientLobbyJoinResponseMessage();
            response.localPlayerId = playerId;
            response.playersInLobby = new List<LobbyPlayer>(Players);
            response.ascension = Ascension;
            response.maxAscension = MaxAscension;
            response.seed = Seed;
            response.modifiers = new List<ModifierDefinition>(_modifiers);
            NetService.SendMessage(response, playerId);

            NetHostGameService hostGameService = NetService as NetHostGameService;
            if (hostGameService != null)
            {
                hostGameService.SetPeerReadyForBroadcasting(playerId);
            }
        }

        private void OnDisconnectedFromClientAsHost(ulong playerId, NetErrorInfo info)
        {
            _logger.Info($"Client {playerId} disconnected: {info.Reason}");
            RemovePlayer(playerId);

            PlayerLeftMessage message = new PlayerLeftMessage();
            message.playerId = playerId;
            NetService.SendMessage(message);
        }

        private void OnDisconnected(NetErrorInfo info)
        {
            _logger.Info($"Disconnected from lobby: {info.Reason}");
            LobbyListener.LocalPlayerDisconnected(info);
        }

        private void BeginRunLocally(string seed, IReadOnlyList<ModifierDefinition> modifiers)
        {
            _isBeginningRun = true;

            List<Player> runPlayers = new List<Player>();
            foreach (LobbyPlayer lobbyPlayer in Players)
            {
                Player player = Player.CreateForNewRun(lobbyPlayer.character, lobbyPlayer.id);
                if (player != null)
                {
                    runPlayers.Add(player);
                }
            }

            LobbyListener.BeginRun(runPlayers, modifiers, seed, GameMode);
            BeginRun?.Invoke(runPlayers, modifiers, seed, GameMode);
        }

        private LobbyPlayer AddPlayerInFirstAvailableSlot(ulong playerId)
        {
            LobbyPlayer existingPlayer = TryGetPlayerOrNull(playerId);
            if (existingPlayer != null)
            {
                return existingPlayer;
            }

            int slotId = GetFirstAvailableSlotId();
            if (slotId < 0)
            {
                throw new InvalidOperationException("Lobby is full.");
            }

            LobbyPlayer lobbyPlayer = new LobbyPlayer();
            lobbyPlayer.id = playerId;
            lobbyPlayer.slotId = slotId;
            lobbyPlayer.character = GetDefaultCharacter();
            lobbyPlayer.isReady = false;
            lobbyPlayer.maxMultiplayerAscensionUnlocked = 0;
            Players.Add(lobbyPlayer);
            return lobbyPlayer;
        }

        private int GetFirstAvailableSlotId()
        {
            for (int i = 0; i < MaxPlayers; ++i)
            {
                bool slotUsed = Players.Any(player => player.slotId == i);
                if (!slotUsed)
                {
                    return i;
                }
            }

            return -1;
        }

        private LobbyPlayer GetPlayer(ulong playerId)
        {
            LobbyPlayer player = TryGetPlayerOrNull(playerId);
            if (player == null)
            {
                throw new InvalidOperationException($"Player {playerId} is not in the lobby.");
            }

            return player;
        }

        private LobbyPlayer TryGetPlayerOrNull(ulong playerId)
        {
            return Players.Find(player => player.id == playerId);
        }

        private void ChangeCharacter(ulong playerId, CharacterDefinition character, bool isRandomCharacterResolution)
        {
            LobbyPlayer lobbyPlayer = GetPlayer(playerId);
            lobbyPlayer.character = character;
            LobbyListener.PlayerChanged(lobbyPlayer, isRandomCharacterResolution);
        }

        private void RemovePlayer(ulong playerId)
        {
            LobbyPlayer lobbyPlayer = TryGetPlayerOrNull(playerId);
            if (lobbyPlayer == null)
            {
                return;
            }

            Players.Remove(lobbyPlayer);
            LobbyListener.RemotePlayerDisconnected(lobbyPlayer);
            PlayerDisconnected?.Invoke(lobbyPlayer);
        }

        private void NotifyPlayerConnected(LobbyPlayer lobbyPlayer)
        {
            LobbyListener.PlayerConnected(lobbyPlayer);
            PlayerConnected?.Invoke(lobbyPlayer);
        }

        private void BroadcastPlayerJoined(LobbyPlayer lobbyPlayer, ulong skipPlayerId)
        {
            PlayerJoinedMessage message = new PlayerJoinedMessage();
            message.lobbyPlayer = lobbyPlayer;

            foreach (LobbyPlayer player in Players)
            {
                if (player.id == NetService.NetId || player.id == skipPlayerId)
                {
                    continue;
                }

                NetService.SendMessage(message, player.id);
            }
        }

        private void DisconnectClient(ulong playerId, NetError reason)
        {
            NetHostGameService hostGameService = NetService as NetHostGameService;
            if (hostGameService == null)
            {
                return;
            }

            hostGameService.DisconnectClient(playerId, reason);
        }

        private void EnsureHostOrSingleplayer()
        {
            if (NetService.Type == NetGameType.Client)
            {
                throw new InvalidOperationException("Only host or singleplayer lobby can change this state.");
            }
        }

        private CharacterDefinition GetCharacterOrDefault(string characterId)
        {
            CharacterDefinition character =
                DefinitionDB.CharacterDefinitions.FirstOrDefault(definition => definition.Id == characterId);
            if (character != null)
            {
                return character;
            }

            return GetDefaultCharacter();
        }

        private CharacterDefinition GetDefaultCharacter()
        {
            return DefinitionDB.CharacterDefinitions[0];
        }

        private string CreateRandomSeed()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
        }
    }

    [Serializable]
    public class LobbyPlayer
    {
        public ulong id;
        public int slotId;
        public CharacterDefinition character;
        public bool isReady;
        public int maxMultiplayerAscensionUnlocked;
    }
}