using STSLite.Core.Models;
using STSLite.Core.Multiplayer.Game.Lobby;
using System;
using System.Collections.Generic;
using System.Text;
using STSLite.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STSLite.UI
{
    public class UICharacterSlotWidget : MonoBehaviour
    {
        [SerializeField] private Button _buttonSelect;
        [SerializeField] private TMP_Text _textName;
        [SerializeField] private Image _imagePortrait;
        [SerializeField] private TMP_Text _textStats;
        [SerializeField] private TMP_Text _textPlayers;
        [SerializeField] private GameObject _objectSelected;

        private CharacterDefinition _character;
        private Action<CharacterDefinition> _onSelected;

        public void Bind(CharacterDefinition character, Action<CharacterDefinition> onSelected)
        {
            _character = character;
            _onSelected = onSelected;

            gameObject.SetActive(true);
            SetText(_textName, character.Name);
            SetText(_textStats, $"HP {character.BaseHealth}  ATK {character.BaseAttack}  DEF {character.BaseDefense}");
            SetText(_textPlayers, string.Empty);
            SetImage(character.IconPath.ToSprite());

            if (_buttonSelect != null)
            {
                _buttonSelect.onClick.RemoveListener(OnSelected);
                _buttonSelect.onClick.AddListener(OnSelected);
            }
        }

        public void Clear()
        {
            _character = null;
            _onSelected = null;

            if (_buttonSelect != null)
            {
                _buttonSelect.onClick.RemoveListener(OnSelected);
            }

            gameObject.SetActive(false);
        }

        public void SetSelected(CharacterDefinition selectedCharacter)
        {
            bool selected = _character != null && selectedCharacter != null && _character.Id == selectedCharacter.Id;
            if (_objectSelected)
            {
                _objectSelected.SetActive(selected);
            }
        }

        public void SetLobbyPlayers(IReadOnlyList<LobbyPlayer> players, ulong localNetId)
        {
            if (_character == null || players == null)
            {
                SetText(_textPlayers, string.Empty);
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (LobbyPlayer player in players)
            {
                if (player.character == null || player.character.Id != _character.Id)
                {
                    continue;
                }

                string localText = player.id == localNetId ? "You" : $"P{player.slotId + 1}";
                string readyText = player.isReady ? "Ready" : "Choosing";
                stringBuilder.AppendLine($"{localText}: {readyText}");
            }

            SetText(_textPlayers, stringBuilder.ToString());
        }

        public void SetInteractable(bool interactable)
        {
            if (_buttonSelect != null)
            {
                _buttonSelect.interactable = interactable && _character != null;
            }
        }

        private void OnSelected()
        {
            if (_character != null)
            {
                _onSelected?.Invoke(_character);
            }
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void SetImage(Sprite sprite)
        {
            if (_imagePortrait)
            {
                _imagePortrait.sprite = sprite;
            }
        }
    }
}