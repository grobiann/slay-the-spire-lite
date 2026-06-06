using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace STSLite.UI
{
    public class UITopBar : UIBase
    {
        public static UITopBar Instance { get; private set; }

        [FormerlySerializedAs("_textHP")]
        [SerializeField] private TMP_Text _textHp;

        [FormerlySerializedAs("_textGold")]
        [SerializeField] private TMP_Text _textGold;

        // TODO: Positions
        // TODO: Current Stage Info
        // TODO: Deck, Setttings, etc.
        // TODO: Relics
    }
}
