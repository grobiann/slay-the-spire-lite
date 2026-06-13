using System.Collections.Generic;
using STSLite.Core.Multiplayer.Game;
using STSLite.Core.Multiplayer.Game.Lobby;
using UnityEngine;

namespace STSLite.UI
{
    public class UILobbyPlayerContainer : UIBase
    {
        [SerializeField] private SlotContainer<UILobbyPlayerSlot> _lobbyPlayerSlots;
        private StartRunLobby _lobby;

        public void Initialize(StartRunLobby lobby)
        {
            _lobbyPlayerSlots.SetSize(0);
            if (!lobby.NetService.Type.IsMultiplayer())
            {
                return;
            }

            _lobby = lobby;
            foreach (LobbyPlayer player in _lobby.Players)
            {
                OnPlayerConnected(player);
            }
        }

        public void OnPlayerConnected(LobbyPlayer player)
        {
            if (!ShouldDisplayPlayer(player))
            {
                return;
            }

            var prevSize = _lobbyPlayerSlots.Count;
            _lobbyPlayerSlots.SetSize(prevSize + 1);
            _lobbyPlayerSlots[prevSize].Setup(player);
        }

        public void OnPlayerDisconnected(LobbyPlayer player)
        {
            if (!ShouldDisplayPlayer(player))
            {
                return;
            }

            for (int i = 0; i < _lobbyPlayerSlots.Count; i++)
            {
                var slot = _lobbyPlayerSlots[i];
                if (slot.LobbyPlayer == player)
                {
                    _lobbyPlayerSlots.RemoveSlot(slot);
                    return;
                }
            }
        }

        public void OnPlayerChanged(LobbyPlayer player)
        {
            if (!ShouldDisplayPlayer(player))
            {
                return;
            }

            for (int i = 0; i < _lobbyPlayerSlots.Count; i++)
            {
                var slot = _lobbyPlayerSlots[i];
                if (slot.LobbyPlayer == player)
                {
                    slot.Setup(slot.LobbyPlayer);
                    return;
                }
            }
        }

        private bool ShouldDisplayPlayer(LobbyPlayer player)
        {
            return true;
            //return _lobby != null && player.id != _lobby.LocalPlayer.id;
        }
    }
}