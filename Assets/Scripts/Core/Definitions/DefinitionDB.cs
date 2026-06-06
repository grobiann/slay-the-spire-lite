using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace STSLite.Core.Models
{
    public record AbstractDefinitionId
    {
        public string Category;
        public string Entry;
    }

    public abstract class AbstractDefinition
    {
    }

    public class DefinitionDB
    {
        public static IReadOnlyList<CharacterDefinition> CharacterDefinitions = new List<CharacterDefinition>()
        {
            new CharacterDefinition()
            {
                Id = "The Ironclad",
                Name = "The Ironclad",
                Description = "A mighty warrior with high health and strong attacks.",
                BaseHealth = 80,
                BaseAttack = 10,
                BaseDefense = 5
            },
        };

        public static IReadOnlyList<ModifierDefinition> RunModifierDefinitions = new List<ModifierDefinition>()
        {
            // Add RunModifierDefinition instances here.
        };

        public static IReadOnlyList<PotionDefinition> PotionDefinitions = new List<PotionDefinition>()
        {
            // Add PotionDefinition instances here.
        };

        public static IReadOnlyList<OrbDefinition> OrbDefinitions = new List<OrbDefinition>()
        {
            // Add OrbDefinition instances here.
        };

        public static IReadOnlyList<RelicDefinition> RelicDefinitions = new List<RelicDefinition>()
        {
            // Add RelicDefinition instances here.
        };

        public static IReadOnlyList<MonsterDefinition> MonsterDefinitions = new List<MonsterDefinition>()
        {
            // Add MonsterDefinition instances here.
        };

        public static IReadOnlyList<EncounterDefinition> EncounterDefinitions = new List<EncounterDefinition>()
        {
            // Add EncounterDefinition instances here.
        };

        public static IReadOnlyList<EventDefinition> EventDefinitions = new List<EventDefinition>()
        {
            // Add EventDefinition instances here.
        };

        public static IReadOnlyList<CardDefinition> CardDefinitions = new List<CardDefinition>()
        {
            // Add CardDefinition instances here.
        };

        public static IReadOnlyList<ActDefinition> ActDefinitions = new List<ActDefinition>()
        {
            Act<Act1>(),
            Act<Act2>(),
            Act<Act3>()
        };

        public static IReadOnlyList<RoomDefinition> RoomDefinitions = new List<RoomDefinition>()
        {
            // Add RoomDefinition instances here.
        };

        private static Dictionary<AbstractDefinitionId, AbstractDefinition> _definitionById = new Dictionary<AbstractDefinitionId, AbstractDefinition>();

        public static void Init()
        {
            _definitionById.Clear();

            IEnumerable<Type> definitionTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(AbstractDefinition)));

            foreach (Type definitionType in definitionTypes)
            {
                AbstractDefinitionId definitionId = GetId(definitionType);
                AbstractDefinition definition = (AbstractDefinition)Activator.CreateInstance(definitionType);
                _definitionById.Add(definitionId, definition);
            }
        }

        public static ActDefinition Act<T>() where T : ActDefinition, new()
        {
            return new T();
        }

        public static RoomDefinition Room<T>() where T : RoomDefinition, new()
        {
            return new T();
        }

        public static AbstractDefinitionId GetId(Type type)
        {
            return new AbstractDefinitionId()
            {
                Category = GetCategory(type),
                Entry = GetEntry(type)
            };
        }

        public static string GetCategory(Type type)
        {
            Type categoryType = type;
            while(categoryType.BaseType != typeof(AbstractDefinition))
            {
                categoryType = categoryType.BaseType;
            }

            return StringHelper.Slugify(categoryType.Name);
        }

        public static string GetEntry(Type type)
        {
            return StringHelper.Slugify(type.Name);
        }
    }

    public class CharacterDefinition : AbstractDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int BaseHealth { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
    }


    public class ModifierDefinition : AbstractDefinition
    {

    }

    public class PotionDefinition : AbstractDefinition
    {

    }

    public class OrbDefinition : AbstractDefinition
    {

    }

    public class RelicDefinition : AbstractDefinition
    {

    }

    public class MonsterDefinition : AbstractDefinition
    {

    }

    public class EncounterDefinition : AbstractDefinition
    {

    }

    public class EventDefinition : AbstractDefinition
    {

    }

    public class CardDefinition : AbstractDefinition
    {
        
    }

    public class RoomDefinition : AbstractDefinition
    {

    }

    public class MapRoomDefinition : RoomDefinition
    {
    }
}
