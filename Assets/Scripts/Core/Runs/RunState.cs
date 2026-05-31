using STSLite.Core.Maps;
using STSLite.Core.Models;
using STSLite.Core.Random;
using System.Collections.Generic;
using System.Linq;

namespace STSLite.Core.Runs
{
    public enum EPileType
    {
        Deck,
        Hand,
        Discard,
        Exhaust,
    }

    public class CardPile
    {
        public EPileType PileType { get; }
        public IReadOnlyList<Card> Cards { get; } = new List<Card>();

        public CardPile(EPileType pileType)
        {
            PileType = pileType;
        }
    }

    public class RunRngSet
    {
        public Rng UnknownMapPoint { get; private set; }
        public string Seed { get; }

        public RunRngSet(string seed)
        {
            UnknownMapPoint = new Rng();
            Seed = seed;
        }
    }

    public class RunOddsSet
    {
        public RunOddsSet(Rng rng)
        {
        }
    }

    public class RunState
    {
        //public int NextRoomID { get; private set; }

        public ActDefinition Act => Acts[CurrentActIndex];

        public bool IsGameOver { get; private set; }

        public IReadOnlyList<Player> Players { get; }
        public IReadOnlyList<ActDefinition> Acts { get; }
        public IReadOnlyList<ModifierDefinition> Modifiers { get; }
        public EGameMode GameMode { get; }

        public RunRngSet RngSet { get; }
        public RunOddsSet OddsSet { get; }

        public int CurrentActIndex { get; set; }
        public int CurrentRoomCount;
        private readonly List<Card> _allCards = new List<Card>();
        private readonly List<Room> _currentRooms = new();
        public ActMap Map { get; set; }


        public RunState(IReadOnlyList<Player> players, IReadOnlyList<ActDefinition> acts, IReadOnlyList<ModifierDefinition> modifiers, EGameMode gameMode, int currentActIndex, RunRngSet rng, RunOddsSet odds)
        {
            Players = players;
            Acts = acts;
            Modifiers = modifiers;
            GameMode = gameMode;
            CurrentActIndex = currentActIndex;
            RngSet = rng;
            OddsSet = odds;
        }

        public static RunState CreateForNewRun(IReadOnlyList<Player> players, IReadOnlyList<ActDefinition> acts, IReadOnlyList<ModifierDefinition> modifiers, EGameMode gameMode, string seed)
        {
            RunRngSet rngSet = new RunRngSet(seed);
            RunOddsSet oddsSet = new RunOddsSet(rngSet.UnknownMapPoint);
            RunState runState = CreateShared(players, acts, modifiers, gameMode, 0, rngSet, oddsSet);
            foreach (Player player in players)
            {
                player.InitializeSeed(seed);
            }
            return runState;
        }

        public static RunState CreateShared(IReadOnlyList<Player> players, IReadOnlyList<ActDefinition> acts, IReadOnlyList<ModifierDefinition> modifiers, EGameMode gameMode, int currentActIndex, RunRngSet rng, RunOddsSet odds)
        {
            var runState = new RunState(players, acts, modifiers, gameMode, currentActIndex, rng, odds);
            foreach (Player player in players)
            {
                player.RunState = runState;
                foreach (Card card in player.Deck.Cards)
                {
                    runState.AddCard(card, player);
                    card.AfterCreated();
                }
            }

            return runState;
        }

        public Card CreateCard(CardDefinition canonicalCard, Player owner)
        {
            Card card = new Card(canonicalCard);
            AddCard(card, owner);
            card.AfterCreated();
            return card;
        }

        public void AddCard(Card card, Player owner)
        {
            card.Owner = owner;
            _allCards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            card.Owner = null;
            _allCards.Remove(card);
        }

        public bool ContainCard(Card card)
        {
            return _allCards.Contains(card);
        }

        public void PushRoom(Room room)
        {
            if (_currentRooms.Contains(room))
            {
                throw new System.InvalidOperationException("Cannot push a room that is already in the current rooms.");
            }

            _currentRooms.Add(room);
        }

        public Room PopCurrentRoom()
        {
            if (_currentRooms.Count == 0)
            {
                throw new System.InvalidOperationException("Cannot pop room when there is no current room.");
            }
            Room result = _currentRooms.Last();
            _currentRooms.RemoveAt(_currentRooms.Count - 1);
            return result;
        }
    }
}