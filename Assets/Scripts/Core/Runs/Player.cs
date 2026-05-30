using STSLite.Core.Models;
using System.Collections.Generic;

namespace STSLite.Core.Runs
{
    public class Player
    {
        public IReadOnlyList<RelicDefinition> Relics { get; private set; }
        public IReadOnlyList<PotionDefinition?> Potions { get; private set; }
        public int Gold { get; private set; }
        public int MaxEnergy { get; private set; }
        public int MaxPotionCount => Potions.Count;
        public CharacterDefinition Character { get; private set; }
        public CardPile RunCardPile { get; private set; }
        public RunState RunState { get; set; }
        public CardPile Deck { get; } = new CardPile(EPileType.Deck);
        public ulong NetId { get; }

        public Player(CharacterDefinition character, ulong netId, int currentHp, int maxHp, int maxEnergy, int gold, int potionSlotCount, int orbSlotCount, 
            List<AbstractDefinitionId> discoveredCards = null,
            List<AbstractDefinitionId> discoveredEnemies = null,
            List<AbstractDefinitionId> discoveredEpochs = null,
            List<AbstractDefinitionId> discoveredRelics = null)
        {
            Character = character;
            NetId = netId;
            // TODO:
        }

        public static Player CreateForNewRun(CharacterDefinition character, ulong netId)
        {
            Player player = new Player(
                    character: character,
                    netId: netId,
                    currentHp: character.BaseHealth,
                    maxHp: character.BaseHealth,
                    maxEnergy: 3,
                    gold: 99,
                    potionSlotCount: 3,
                    orbSlotCount: 0
                );
            return player;
        }

        public void InitializeSeed(string seed)
        {
        }
    }
}