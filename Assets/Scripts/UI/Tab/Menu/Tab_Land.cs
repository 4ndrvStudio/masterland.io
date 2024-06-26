using System.Collections.Generic;
using UnityEngine;
using System;

namespace masterland.UI
{
    using Data;
    using TMPro;
    using UnityEngine.UI;
    using Wallet;
    public class Tab_Land : UITab
    {
        public List<LandSlot> LandSlots = new();
        [SerializeField] private GameObject _landOb;
        [SerializeField] private GameObject _container;
        [SerializeField] private GameObject _loadingOb;

        [SerializeField] private GameObject _currentLandOb;
        [SerializeField] private TextMeshProUGUI _currentLandNameText;
        [SerializeField] private TMP_InputField _currentLandAddressText;
        [SerializeField] private Button _unregisterLandBtn;

        public UILandInfo LandInfo;
        
        public void Start() {
            _unregisterLandBtn.onClick.AddListener(Unregister);
        }

        public override void Show()
        {
            base.Show();
            GetLands();
            SetupCurrentPanel();
        }

        public override void Hide() {
            base.Hide();
            _currentLandOb.SetActive(false);
            LandSlots.ForEach(item => Destroy(item.gameObject));
            LandSlots.Clear();
            LandInfo.Hide();
        }

        public void SetupCurrentPanel() {
            _currentLandOb.gameObject.SetActive(Data.Instance.Land != null && !string.IsNullOrEmpty(Data.Instance.Land.Id));
            if(Data.Instance.Land == null)
                return;
            _currentLandNameText.text = Data.Instance.Land.Name;
            _currentLandAddressText.text = Data.Instance.Land.Id.ToShortAddress();
        }

        public async void Unregister() {
             _unregisterLandBtn.interactable = false;
             _unregisterLandBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Processing..";
                ContractRespone contractRespone = await WalletInteractor.Instance.UnregisterResidentLicense(Data.Instance.MasterData.Id);
                if (!contractRespone.IsSucess)
                {
                    Debug.Log("Can't Unregister");
                }

        
            await Data.Instance.GetSelectedMaster();
            _unregisterLandBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Unregister";
            _unregisterLandBtn.interactable = true;
            LandInfo.UpdateRegisterBtnContent();
            SetupCurrentPanel();
        }

        public async void GetLands()
        {
            _loadingOb.SetActive(true);
            Data.Instance.Lands = await WalletInteractor.Instance.GetLands();
            await Data.Instance.GetSelectedMaster();
            _loadingOb.SetActive(false);
            
            foreach (LandData land in Data.Instance.Lands)
            {
                var landSlot = Instantiate(_landOb, _container.transform);
                landSlot.SetActive(true);
                var script = landSlot.GetComponent<LandSlot>();
                script.Setup(land,this);
                LandSlots.Add(script);
            }

            
        }



    }
}
