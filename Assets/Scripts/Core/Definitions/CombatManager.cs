using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using STSLite.Core.Runs;

namespace STSLite.Core.Models
{
    public class CombatManager
    {
        public static CombatManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CombatManager();
                }

                return _instance;
            }
        }

        private static CombatManager _instance;

        private readonly HashSet<Player> _playersReadyToEndTurn = new();
        private readonly HashSet<Player> _playersReadyToBeginEnemyTurn = new();
        private readonly List<Player> _playersTakingExtraTurn = new();
        private CombatState _combatState;
        private CancellationTokenSource _combatCts;
        private bool _playerActionDisabled;

        public bool IsPaused { get; private set; }
        public bool IsPlayPhase { get; private set; }
        public bool IsEnemyTurnStarted { get; private set; }

//public CombatStateTracker StateTracker { get; }
//public CombatHistory History { get; }

        public bool IsEnding;
        public bool IsOverOrEnding;


        public event Action<CombatState> CombatSetUp;
        public event Action<CombatRoom> CombatEnded;
        public event Action<CombatRoom> CombatWin;
        public event Action<CombatState> CreaturesChanged;
        public event Action<CombatState> TurnStarted;
        public event Action<CombatState> TurnEnded;
        public event Action<Player, bool> PlayerEndedTurn;
        public event Action<Player> PlayerUnendedTurn;
        public event Action<CombatState> AboutToSwitchToEnemyTurn;
        public event Action<CombatState> PlayerActionsDisabledChanged;


        public void StartCombat(CombatState state)
        {
            _combatState = state;
            //_combatState.MultiplayerScalingModel
            //StateTracker.SetState(state);
            // using (_playerReadlyLock.EnterScope())
            // {
            //     _playerTakingExtraTurn.Clear();
            // }
            foreach (Player player in state.Players)
            {
                player.ResetCombatState();
            }

            foreach (Player player in state.Players)
            {
                player.PopulateCombatState(state);
            }

            foreach (Creature creature in state.Creatures)
            {
                AddCreature(creature);
            }

            CombatSetUp?.Invoke(state);
        }

        public void AfterCombatRoomLoaded()
        {
            
        }

        public async UniTask StartCombatInternal()
        {
            
        }

        private async UniTask StartTurn()
        {
        }

        private async UniTask SetupPlayerTurn(Player player)
        {
            
        }

        public void SetReadyToEndTurn(Player player)
        {
            
        }

        public void UndoReadyToEndTurn(Player player)
        {
            
        }

        public void OnEndedTurnLocally()
        {
            
        }

        public void SetReadyToBeginEnemyTurn(Player player)
        {
            
        }

        public bool IsPlayerReadyToEndTurn(Player player)
        {
            return false;
        }

        public bool IsAllPlayersReadyToEndTurn()
        {
            return false;
        }

        private async UniTask EndEnemyTurn()
        {
            
        }

        public void AddCreature(Creature creature)
        {
            
        }

        public async UniTask AfterCreatureAdded(Creature creature)
        {
            
        }

        public void RemoveCreature(Creature creature)
        {
            
        }

        public void Reset()
        {
        }

        public async UniTask HandlePlayerDeath(Player player)
        {
            
        }

        public void LoseCombat()
        {
            
        }

        private async UniTask EndCombatInternal()
        {
            
        }

        private bool CheckWinCondition()
        {
            
        }

        private async UniTask ExecuteEnemyTurn()
        {
            
        }

        private async UniTask WaitForActionThenEndTurn()
        {
            
        }

        private async UniTask AfterAllPlayersReadyToEndTurn()
        {
            
        }

        private async UniTask WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction()
        {
            
        }

        private async UniTask EndPlayerTurnPhaseOneInternal()
        {
            
        }

        private async UniTask DoTurnEnd(Player player)
        {
            
        }

        private async UniTask EndEnemyTurnInternal()
        {
            
        }
        
        private async UniTask AfterAllPlayersReadyToBeginEnemyTurn()
        {
            
        }

        public async UniTask SwitchFromPlayerToEnemySide()
        {
            
        }

        private void SwitchSides()
        {
            
        }

        public void Pause()
        {
            
        }

        public void Unpause()
        {
            
        }

        public bool IsPartOfPlayerTurn(Player player)
        {
            return false;
        }

        public async UniTask WaitForUnpause()
        {
            
        }
    }
}