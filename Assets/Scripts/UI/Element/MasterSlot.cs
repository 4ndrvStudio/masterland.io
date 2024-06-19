
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using Data;

    public class MasterSlot : MonoBehaviour
    {
        public Button ChooseBtn;
        public GameObject ChoosedBtn;
        public GameObject LoadingOb;
        [SerializeField] private TextMeshProUGUI _masterNameText;
        [SerializeField] private TMP_InputField _addressText;
        [HideInInspector] public MasterData MasterData;
        private Tab_Master _tab_Master;

        public void Start() {
            ChooseBtn.onClick.AddListener(() => Choose());
        }


        public void Setup(MasterData masterData, Tab_Master tab_Master) 
        {
            MasterData = masterData;
            _tab_Master = tab_Master;
            _masterNameText.text = masterData.Name;
            _addressText.text = masterData.Id.ToShortAddress();
            ChooseBtn.gameObject.SetActive(Data.Instance.MasterData.Id != masterData.Id);
            ChoosedBtn.SetActive(Data.Instance.MasterData.Id == masterData.Id);
        }

        public async void Choose() 
        {
            LoadingOb.SetActive(true);
            ChooseBtn.gameObject.SetActive(false);
            string selectedMaster = await Data.Instance.SelectMaster(MasterData.Id);
            LoadingOb.SetActive(false);

            if(selectedMaster == null || selectedMaster != MasterData.Id) {
                ChooseBtn.gameObject.SetActive(true);
            }
            else {
                ChooseBtn.gameObject.SetActive(false);
                ChoosedBtn.SetActive(true);
                _tab_Master.SetupCurrentPanel();
                _tab_Master.MasterSlots.ForEach(item => {
                    if(item != this)
                        item.UnChoose();
                });
            }
             
        }

        public void UnChoose() {
            ChooseBtn.gameObject.SetActive(true);
            ChoosedBtn.SetActive(false);
        }

    }
}
