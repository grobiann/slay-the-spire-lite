using STSLite.Core.Models;
using UnityEngine;

namespace STSLite.UI
{
    public class UICombatRoom : UIBase
    {
        public static UICombatRoom Instance { get; private set; }

        [SerializeField] private Transform _playerParent;
        [SerializeField] private Transform _monsterParent;
        [SerializeField] private Transform _cardParent;

        public void SetupRoom(CombatRoom room)
        {
            CreatePlayers();
            CreateMonsters();
            CreateCards();
        }

        private void CreatePlayers()
        {
            string prefabPath = "Prefabs/Players/player_0";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            Instantiate(prefab, _playerParent);
        }

        private void CreateMonsters()
        {
            string prefabPath = "Prefabs/Monsters/monster_0";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            Instantiate(prefab, _monsterParent);
        }

        private void CreateCards()
        {
            string prefabPath = "Prefabs/Cards/card_0";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            Instantiate(prefab, _cardParent);
        }
    }
}