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

        public void StartCombat(CombatState combatState)
        {
        }

        public void Reset()
        {
        }
    }
}