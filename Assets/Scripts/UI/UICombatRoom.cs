using STSLite.Core.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STSLite.UI
{
    public class UIPlayerHand : MonoBehaviour
    {
        
    }

    public class UICardHolder : MonoBehaviour
    {
        
    }
    
    
    public class UICombatRoom : UIBase
    {
        public static UICombatRoom Instance { get; private set; }

        [SerializeField] private Transform _playerParent;
        [SerializeField] private Transform _monsterParent;
        [SerializeField] private Transform _cardParent;

        [SerializeField] private Button _buttonEndTrun;
        [SerializeField] private TMP_Text _textEnergy;
        [SerializeField] private Button _buttonDrawPile;
        [SerializeField] private Button _buttonDiscardPile;
        [SerializeField] private Button _buttonExhaustPile;
        [SerializeField] private UIPlayerHand _playerHand;

        public void Activate(CombatRoom combatState)
        {
            
        }

        public void Deactivate()
        {
        }

        private void OnCombatEnded(CombatRoom room)
        {
            
        }

        private void OnCombatWin(CombatRoom room)
        {
            
        }

        private void AnimIn()
        {
            
        }

        private void AnimOut()
        {
            
        }

        private void PostCombatCleanUp()
        {
            
        }

        public void OnHandSelectModeEntered()
        {
            
        }

        public void OnHandSelectModeExited()
        {
            
        }

        private void RegisterCombatEvents()
        {
            CombatManager.Instance.CombatSetUp += OnCombatSetup;
            CombatManager.Instance.CombatEnded += RestrictControllerNavigation;
            CombatManager.Instance.CombatWin += RestrictControllerNavigation;
        }

        private void UnregisterCombatEvents()
        {
            CombatManager.Instance.CombatSetUp -= OnCombatSetup;
            CombatManager.Instance.CombatEnded -= RestrictControllerNavigation;
            CombatManager.Instance.CombatWin -= RestrictControllerNavigation;
        }

        private void OnCombatSetup(CombatState state)
        {
            // Setup Background
            
            CreatePlayers();
            CreateMonsters();
            CreateCards();
        }

        private void RestrictControllerNavigation()
        {
            
        }

        public void SetupRoom(CombatRoom room)
        {
            
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