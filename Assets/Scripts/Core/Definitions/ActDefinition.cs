using STSLite.Core.Saves;
using System.Collections.Generic;

namespace STSLite.Core.Models
{
    public class ActDefinition : AbstractDefinition
    {
        public string Title;

        public static IReadOnlyList<ActDefinition> GetDefaultList()
        {
            return new List<ActDefinition>(new ActDefinition[3]
            {
                DefinitionDB.Act<Act1>(),
                DefinitionDB.Act<Act2>(),
                DefinitionDB.Act<Act3>()
            });
        }
    }

    public class Act1 : ActDefinition
    {
    }

    public class Act2 : ActDefinition
    {

    }

    public class Act3 : ActDefinition
    {

    }
}
