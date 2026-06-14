using Cysharp.Threading.Tasks;
using STSLite.Core.Models;
using STSLite.Core.Runs;
using STSLite.Core.Saves;
using STSLite.UI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STSLite.Core.Multiplayer.Game;
using UnityEditorInternal;
using UnityEngine;

namespace STSLite.Core
{
    public class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }

        public UIRemoteMouseCursorContainer UIRemoteCursorContainer { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Debug.LogError("Game instance already exists.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameStartup().Forget();
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private async UniTask GameStartup()
        {
            UIRemoteCursorContainer = UIManager.Instance.Show<UIRemoteMouseCursorContainer>();

            // GitHelper.Initialize();
            SaveManager.Instance.TryFirstTimeCloudSync();
            SaveManager.Instance.SyncCloudToLocal();

            OneTimeInitialization.ExecuteEssential();

            SettingsSaveData settingsSaveData = SaveManager.Instance.SettingsSaveData;
            AudioManager audioManager = AudioManager.Instance;
            audioManager.SetMasterVolume(settingsSaveData.VolumeMaster);
            audioManager.SetBGMVolume(settingsSaveData.VolumeBGM);
            audioManager.SetSFXVolume(settingsSaveData.VolumeSFX);

            // LeaderboardManager.Instance.Initialize();
            // SteamStatsManager.Instance.Initialize();

            // SaveManager.Instance.InitProfileId();
            // SaveManager.Isntance.InitProgressData();
            // SaveManager.Instance.InitPrefsData;

            // AutoSlay모드
            // Bootstrap

            bool skipLogo = true;
            LaunchMainMenu(skipLogo).Forget();
        }

        private async UniTask LaunchMainMenu(bool skipLogo)
        {
            if (!skipLogo)
            {
                await UILogo.ShowLogo();
            }

            UIManager.Instance.Show<UIMainMenu>();
            await UIBlackScreen.Off();
        }

        public async UniTask StartNewSinglePlayerRun(CharacterDefinition character,
            IReadOnlyList<ModifierDefinition> modifiers, string seed, GameMode gameMode)
        {
            RunState runState = RunState.CreateForNewRun(
                players: new List<Player> { Player.CreateForNewRun(character, 1uL) },
                acts: DefinitionDB.ActDefinitions,
                modifiers: modifiers,
                gameMode: gameMode,
                seed: seed);
            RunManager.Instance.SetupNewSinglePlayer(runState);

            //await StartRun(runState);

            await PreloadManager.LoadRunAssets();
            await PreloadManager.LoadActAssets(runState.Act);
            await RunManager.Instance.FinalizeStartingRelics();
            RunManager.Instance.Launch();
            await RunManager.Instance.EnterAct(0);
        }
    }

    public enum GameMode
    {
        None,
        Standard,
        Daily,
        Custom
    }
}