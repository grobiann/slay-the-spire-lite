using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using STSLite.Core.Maps;
using STSLite.Core.Runs;
using STSLite.UI;
using System.Threading.Tasks;

namespace STSLite.Core.Models
{
    public class CombatRoom : Room
    {
        public override RoomType RoomType => throw new System.NotImplementedException();
        public CombatState CombatState { get; }

        public CombatRoom(RunState runstate)
        {
            CombatState = new CombatState(runstate);
        }


        public override async UniTask EnterInternal(RunState runState)
        {
            await StartCombat(runState);
        }

        public override UniTask Exit()
        {
            //CombatManager.Instance.Reset();
            return UniTask.CompletedTask;
        }

        public override UniTask Resume()
        {
            throw new System.NotImplementedException();
        }

        private async UniTask StartCombat(RunState runState)
        {
            //List<Monster> monsters = MonsterFactory.Instance.CreateMonstersForRoom(runState);
            //await CombatManager.Instance.StartCombat(monsters, runState);

            UICombatRoom combatRoomUI = UIManager.Instance.Show<UICombatRoom>();
            combatRoomUI.SetupRoom(this);

            CombatManager.Instance.StartCombat(CombatState);
        }

        public void OnCombatEnded()
        {
        }
    }

    public class CombatState
    {
        public RunState RunState { get; }
        public IReadOnlyList<Creature> Allies { get; }
        public IReadOnlyList<Creature> Enemies { get; }
        public IReadOnlyList<Creature> Creatures { get; }
        public IReadOnlyList<Player> Players { get; }
        public IReadOnlyList<Creature> PlayerCreatures { get; }
        public IReadOnlyList<ModifierDefinition> Modifiers { get; }
        public IReadOnlyList<Creature> EscapedCreatures { get; }
        public int RoundNumber { get; set; }
        public CombatSide CurrentSide { get; set; }

        public event Action<CombatState> CreaturesChanged;
        
        public CombatState(RunState runState)
        {
            RunState = runState;
            RoundNumber = 1;
            CurrentSide = CombatSide.Player;
            RunState = runState;
        }

        public Card CreateCard(CardDefinition cardDefinition, Player owner)
        {
            return null;
        }

        public void AddCard(Card card)
        {
        }

        public bool ContainsCard(Card card)
        {
            return false;
        }

        public void AddPlayer(Player player)
        {
        }

        public void CreateCreature(MonsterDefinition monsterDefinition, CombatSide side, string slot)
        {
        }

        public void RemoveCreature(Creature creature)
        {
        }

        public bool ContainsCreature(Creature creature)
        {
            return false;
        }

        public bool ContainsMonster(MonsterDefinition monsterDefinition)
        {
            return false;
        }

        public void SetEnemyIndex(Creature creature, int index)
        {
        }
    }


    public enum CombatSide
    {
        None,
        Player,
        Enemy
    }

    public class Creature
    {
    }
}