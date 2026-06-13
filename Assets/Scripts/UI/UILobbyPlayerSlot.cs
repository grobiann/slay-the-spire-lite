using STSLite.Core;
using STSLite.Core.Multiplayer.Game.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STSLite.UI
{
    public class UILobbyPlayerSlot : UIBase
    {
        [SerializeField] private TMP_Text _textName;
        [SerializeField] private TMP_Text _textCharacterTypeName;
        [SerializeField] private Image _imageCharacterType;
        [SerializeField] private GameObject _objectReady;

        public LobbyPlayer LobbyPlayer { get; private set; }

        public void Setup(LobbyPlayer lobbyPlayer)
        {
            if (_textName)
            {
                _textName.text = $"LobbyPlayer_{lobbyPlayer.id}";
            }

            if (_textCharacterTypeName)
            {
                _textCharacterTypeName.text = lobbyPlayer.character.Name;
            }

            if (_imageCharacterType)
            {
                _imageCharacterType.sprite = lobbyPlayer.character.IconPath.ToSprite();
            }

            if (_objectReady)
            {
                _objectReady.SetActive(lobbyPlayer.isReady);
            }
        }
    }
}