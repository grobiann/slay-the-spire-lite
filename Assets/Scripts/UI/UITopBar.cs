using TMPro;
using UnityEngine;

namespace STSLite.UI
{
    public class UITopBar : UIBase
    {
        public static UITopBar Instance { get; private set; }

        [SerializeField] private TMP_Text _textHP;
        [SerializeField] private TMP_Text _textGold;

        // TODO: Positions
        // TODO: Current Stage Info
        // TODO: Deck, Setttings, etc.
        // TODO: Relics
    }
}
