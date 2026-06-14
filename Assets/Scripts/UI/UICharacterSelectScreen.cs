using Cysharp.Threading.Tasks;
using STSLite.Core;
using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Models;
using STSLite.Core.Multiplayer.Game;
using STSLite.Core.Multiplayer.Game.Lobby;
using STSLite.Core.Multiplayer.Messages.Lobby;
using STSLite.Core.Multiplayer.Quality;
using STSLite.Core.Multiplayer.Serialization;
using STSLite.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace STSLite.UI
{
    public class UICharacterSelectScreen : UIBase, IStartRunLobbyListener
    {
        public static UICharacterSelectScreen Instance { get; private set; }

        [SerializeField] private UILobbyPlayerContainer _lobbyPlayerContainer;
        [SerializeField] private SlotContainer<UICharacterSlotWidget> _characterSlots;
        [SerializeField] private Text _textTitle;
        [SerializeField] private Text _textDescription;
        [SerializeField] private Text _textPlayers;
        [SerializeField] private Text _textStatus;
        [SerializeField] private Button _buttonReady;
        [SerializeField] private Button _buttonUnready;
        [SerializeField] private Button _buttonBack;

        private StartRunLobby _lobby;
        private CharacterDefinition _selectedCharacter;

        public override void Begin()
        {
            base.Begin();
            BindCharacterSlots();
            BindButtons();
            SetInteractable(false);
            SetUnreadyVisible(false);
        }

        public override void Finish()
        {
            CleanUpLobby(true);
            base.Finish();
        }

        private void Update()
        {
            if (_lobby != null && _lobby.NetService.IsConnected)
            {
                _lobby.NetService.Update();
            }
        }

        public void InitializeSingleplayer()
        {
            CleanUpLobby(false);
            _lobby = new StartRunLobby(GameMode.Standard, new SingleplayerGameServiceAdapter(), this, 1);
            _lobby.AddLocalHostPlayer();
            SelectCharacter(DefinitionDB.CharacterDefinitions[0]);
            RefreshView();
            SetInteractable(true);
        }

        public void InitializeMultiplayerAsHost(INetGameService gameService, int maxPlayers)
        {
            if (gameService.Type != NetGameType.Host)
            {
                throw new ArgumentException("gameService must be a host game service.");
            }

            CleanUpLobby(false);
            _lobby = new StartRunLobby(GameMode.Standard, gameService, this, maxPlayers);
            _lobby.AddLocalHostPlayer();
            SelectCharacter(DefinitionDB.CharacterDefinitions[0]);
            RefreshView();
            SetInteractable(true);
            AfterInitialized();
        }

        public void InitializeMultiplayerAsClient(INetGameService gameService)
        {
            if (gameService.Type != NetGameType.Client)
            {
                throw new ArgumentException("gameService must be a client game service.");
            }

            CleanUpLobby(false);
            _lobby = new StartRunLobby(GameMode.Standard, gameService, this, -1);
            SelectCharacter(DefinitionDB.CharacterDefinitions[0]);

            ClientLobbyJoinRequestMessage message = new ClientLobbyJoinRequestMessage();
            message.characterId = _selectedCharacter.Id;
            gameService.SendMessage(message);

            RefreshView();
            SetInteractable(true);
            AfterInitialized();
        }

        public void PlayerConnected(LobbyPlayer player)
        {
            RefreshView();
        }

        public void RemotePlayerDisconnected(LobbyPlayer player)
        {
            RefreshView();
        }

        public void PlayerChanged(LobbyPlayer player, bool isRandomCharacterResolution)
        {
            RefreshView();
        }

        public void AscensionChanged()
        {
            RefreshView();
        }

        public void MaxAscensionChanged()
        {
            RefreshView();
        }

        public void SeedChanged()
        {
            RefreshView();
        }

        public void ModifiersChanged()
        {
            RefreshView();
        }

        public void BeginRun(IReadOnlyList<Player> players, IReadOnlyList<ModifierDefinition> modifiers, string seed,
            GameMode gameMode)
        {
            SetInteractable(false);
            SetText(_textStatus, "Starting run...");
            StartRun(players, modifiers, seed, gameMode).Forget();
        }

        public void LocalPlayerDisconnected(NetErrorInfo info)
        {
            SetText(_textStatus, $"Disconnected: {info.Reason}");
            SetInteractable(false);
        }

        private void AfterInitialized()
        {
            Game.Instance.UIRemoteCursorContainer.Initialize(_lobby.InputSynchronizer,
                _lobby.Players.Select((LobbyPlayer p) => p.id).ToList());
        }

        private async UniTask StartRun(IReadOnlyList<Player> players, IReadOnlyList<ModifierDefinition> modifiers,
            string seed, GameMode gameMode)
        {
            RunState runState =
                RunState.CreateForNewRun(players, DefinitionDB.ActDefinitions, modifiers, gameMode, seed);
            RunManager.Instance.SetupNewSinglePlayer(runState);

            await PreloadManager.LoadRunAssets();
            await PreloadManager.LoadActAssets(runState.Act);
            await RunManager.Instance.FinalizeStartingRelics();

            RunManager.Instance.Launch();
            CleanUpLobby(false);
            UIManager.Instance.Close<UICharacterSelectScreen>();
            await RunManager.Instance.EnterAct(0);
        }

        private void BindCharacterSlots()
        {
            IReadOnlyList<CharacterDefinition> characters = DefinitionDB.CharacterDefinitions;
            _characterSlots.SetSize(characters.Count);
            for (int i = 0; i < characters.Count; ++i)
            {
                UICharacterSlotWidget slot = _characterSlots[i];
                slot.Bind(characters[i], SelectCharacter);
            }
        }

        private void BindButtons()
        {
            if (_buttonReady != null)
            {
                _buttonReady.onClick.RemoveListener(OnReadyClicked);
                _buttonReady.onClick.AddListener(OnReadyClicked);
            }

            if (_buttonUnready != null)
            {
                _buttonUnready.onClick.RemoveListener(OnUnreadyClicked);
                _buttonUnready.onClick.AddListener(OnUnreadyClicked);
            }

            if (_buttonBack != null)
            {
                _buttonBack.onClick.RemoveListener(OnBackClicked);
                _buttonBack.onClick.AddListener(OnBackClicked);
            }
        }

        private void SelectCharacter(CharacterDefinition character)
        {
            _selectedCharacter = character;
            SetText(_textDescription,
                $"{character.Description}\n\nHP {character.BaseHealth}  ATK {character.BaseAttack}  DEF {character.BaseDefense}");

            if (_lobby != null && HasLocalPlayer())
            {
                _lobby.SetLocalCharacter(character);
            }

            RefreshView();
        }

        private void OnReadyClicked()
        {
            if (_lobby == null)
            {
                return;
            }

            SetCharacterSlotsInteractable(false);
            SetButtonInteractable(_buttonReady, false);
            SetUnreadyVisible(_lobby.NetService.Type != NetGameType.Singleplayer);
            SetButtonInteractable(_buttonUnready, true);
            SetText(_textStatus, "Ready. Waiting for the other player...");
            _lobby.SetReady(true);
            RefreshView();
        }

        private void OnUnreadyClicked()
        {
            if (_lobby == null)
            {
                return;
            }

            _lobby.SetReady(false);
            SetUnreadyVisible(false);
            SetButtonInteractable(_buttonReady, true);
            SetCharacterSlotsInteractable(true);
            RefreshView();
        }

        private void OnBackClicked()
        {
            CleanUpLobby(true);
            UIManager.Instance.Close<UICharacterSelectScreen>();
        }

        private void RefreshView()
        {
            SetText(_textTitle, _selectedCharacter == null ? "Choose a Character" : _selectedCharacter.Name);
            SetText(_textPlayers, BuildPlayersText());
            RefreshCharacterSlots();
            _lobbyPlayerContainer.Initialize(_lobby);

            if (_lobby == null)
            {
                SetText(_textStatus, "Lobby is not initialized.");
                SetButtonInteractable(_buttonReady, false);
                return;
            }

            if (!HasLocalPlayer())
            {
                SetText(_textStatus, "Joining lobby...");
                SetButtonInteractable(_buttonReady, false);
                return;
            }

            SetButtonInteractable(_buttonReady, _selectedCharacter != null && !_lobby.LocalPlayer.isReady);

            if (_lobby.IsAboutToBeginGame())
            {
                SetText(_textStatus, "All players ready.");
            }
            else if (_lobby.LocalPlayer.isReady)
            {
                SetText(_textStatus, "Ready. Waiting for the other player...");
            }
            else if (_lobby.NetService.Type == NetGameType.Host && _lobby.Players.Count < 2)
            {
                SetText(_textStatus, $"Hosting lobby. Players {_lobby.Players.Count}/{_lobby.MaxPlayers}.");
            }
            else
            {
                SetText(_textStatus, "Select a character, then embark.");
            }
        }

        private void RefreshCharacterSlots()
        {
            foreach (UICharacterSlotWidget slot in _characterSlots)
            {
                slot.SetSelected(_selectedCharacter);
                slot.SetLobbyPlayers(_lobby?.Players, _lobby?.NetService.NetId ?? 0uL);
            }
        }

        private string BuildPlayersText()
        {
            if (_lobby == null)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (LobbyPlayer player in _lobby.Players)
            {
                string readyText = player.isReady ? "Ready" : "Choosing";
                string localText = player.id == _lobby.NetService.NetId ? " (You)" : string.Empty;
                stringBuilder.AppendLine($"P{player.slotId + 1}{localText}: {player.character.Name} - {readyText}");
            }

            return stringBuilder.ToString();
        }

        private bool HasLocalPlayer()
        {
            if (_lobby == null)
            {
                return false;
            }

            foreach (LobbyPlayer player in _lobby.Players)
            {
                if (player.id == _lobby.NetService.NetId)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetInteractable(bool interactable)
        {
            SetButtonInteractable(_buttonBack, interactable);
            SetButtonInteractable(_buttonReady, interactable);
            SetButtonInteractable(_buttonUnready, interactable);
            SetCharacterSlotsInteractable(interactable);
        }

        private void SetCharacterSlotsInteractable(bool interactable)
        {
            foreach (UICharacterSlotWidget widget in _characterSlots)
            {
                if (widget != null)
                {
                    widget.SetInteractable(interactable);
                }
            }
        }

        private void SetUnreadyVisible(bool visible)
        {
            if (_buttonUnready != null)
            {
                _buttonUnready.gameObject.SetActive(visible);
            }
        }

        private void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void CleanUpLobby(bool disconnectSession)
        {
            if (_lobby == null)
            {
                return;
            }

            _lobby.CleanUp(disconnectSession);
            _lobby = null;
        }

        private sealed class SingleplayerGameServiceAdapter : INetGameService
        {
            public ulong NetId => 1uL;
            public bool IsConnected => false;
            public bool isGameLoading { get; private set; }
            public NetGameType Type => NetGameType.Singleplayer;
            public PlatformType Platform => PlatformType.None;

            public event Action<NetErrorInfo> Disconnected;

            public void SendMessage<T>(T message, ulong playerId) where T : INetMessage
            {
            }

            public void SendMessage<T>(T message) where T : INetMessage
            {
            }

            public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate)
                where T : INetMessage
            {
            }

            public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate)
                where T : INetMessage
            {
            }

            public void Update()
            {
            }

            public void Disconnect(NetError reason, bool now = false)
            {
                Disconnected?.Invoke(new NetErrorInfo(reason, selfInitiated: true));
            }

            public ConnectionStats GetStatsForPeer(ulong peerId)
            {
                return null;
            }

            public void SetGameLoading(bool loading)
            {
                isGameLoading = loading;
            }

            public string GetRawLobbyIdentifier()
            {
                return null;
            }
        }
    }
}