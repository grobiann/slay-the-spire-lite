using Cysharp.Threading.Tasks;
using STSLite.Core;
using STSLite.Core.Entities.Multiplayer;
using STSLite.Core.Models;
using STSLite.Core.Multiplayer;
using STSLite.Core.Multiplayer.Game;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace STSLite.UI
{
    public class UIMainMenu : UIBase
    {
        public static UIMainMenu Instance { get; private set; }

        [FormerlySerializedAs("_btnPlay")]
        [SerializeField] private Button _buttonPlay;

        [FormerlySerializedAs("_btnExit")]
        [SerializeField] private Button _buttonExit;

        [FormerlySerializedAs("_btnHost")]
        [SerializeField] private Button _buttonHost;

        [FormerlySerializedAs("_btnClient")]
        [SerializeField] private Button _buttonClient;

        private const int MultiplayerTestHostPort = 33771;

        private void Awake()
        {
            _buttonPlay.onClick.AddListener(OnPlayClicked);
            _buttonExit.onClick.AddListener(OnExitClicked);

            if (_buttonHost != null)
            {
                _buttonHost.onClick.AddListener(OnHostClicked);
            }

            if (_buttonClient != null)
            {
                _buttonClient.onClick.AddListener(OnClientClicked);
            }
        }

        private void OnPlayClicked()
        {
            UICharacterSelectScreen characterSelectionUI = UIManager.Instance.Show<UICharacterSelectScreen>();
            characterSelectionUI.InitializeSingleplayer();
        }

        private void OnExitClicked()
        {
            Game.Instance.QuitApplication();
        }

        private void OnHostClicked()
        {
            StartHostAsync().Forget();
        }

        private async UniTask StartHostAsync()
        {
            PlatformType platformType = PlatformType.None;

            NetHostGameService netService = new NetHostGameService();
            NetErrorInfo? netErrorInfo = null;

            if (platformType == PlatformType.None)
            {
                netService.StartENetHost(MultiplayerTestHostPort, 4);
            }

            if(!netErrorInfo.HasValue)
            {
                var characterSelectionUI = UIManager.Instance.Show<UICharacterSelectScreen>();
                characterSelectionUI.InitializeMultiplayerAsHost(netService, 4);
            }
        }

        private void OnClientClicked()
        {
            StartClientAsync().Forget();
        }

        private async UniTask StartClientAsync()
        {
            NetClientGameService netService = new NetClientGameService();
            NetErrorInfo? netErrorInfo = netService.StartENetClient("127.0.0.1", MultiplayerTestHostPort);
            if (netErrorInfo != null)
            {
                return;
            }

            UICharacterSelectScreen characterSelectionUI = UIManager.Instance.Show<UICharacterSelectScreen>();
            characterSelectionUI.InitializeMultiplayerAsClient(netService);
        }
    }
}
