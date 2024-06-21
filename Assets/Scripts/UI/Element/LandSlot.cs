using masterland.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    public class LandSlot : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _landNameText;
        
        [HideInInspector] public LandData LandData;
        private Tab_Land _tab_Land;
    
        void Start()
        {
            _button.onClick.AddListener(ShowInfo);
        }

        public void Setup(LandData landData, Tab_Land tab_Land) {
            LandData = landData;
            _tab_Land = tab_Land;
            _landNameText.text = landData.Name;
        }

        public void ShowInfo() {

            _tab_Land.LandInfo.Show(LandData.Id);
        }

       
    }
}
