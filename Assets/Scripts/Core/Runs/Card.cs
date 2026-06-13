using STSLite.Core.Models;

namespace STSLite.Core.Runs
{
    public class Card
    {
        public CardDefinition Definition { get; }
        public Player Owner { get; set; }

        public Card(CardDefinition definition)
        {
            Definition = definition;
        }

        public virtual void AfterCreated()
        {
        }
    }
}