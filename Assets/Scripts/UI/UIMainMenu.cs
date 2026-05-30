using Cysharp.Threading.Tasks;
using STSLite.Core;
using STSLite.Core.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace STSLite.UI
{
    public class UIMainMenu : UIBase
    {
        public static UIMainMenu Instance { get; private set; }

        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnExit;

        private void Awake()
        {
            _btnPlay.onClick.AddListener(OnPlayClicked);
            _btnExit.onClick.AddListener(OnExitClicked);
        }

        private void OnPlayClicked()
        {
            var character = DefinitionDB.CharacterDefinitions[0];
            var modifiers = new List<ModifierDefinition>();
            var seed = System.DateTime.Now.Ticks.ToString();
            Game.Instance.StartNewSinglePlayerRun(character, modifiers, seed, EGameMode.Standard).Forget();
        }

        private void OnExitClicked()
        {
            Game.Instance.QuitApplication();
        }
    }
}
