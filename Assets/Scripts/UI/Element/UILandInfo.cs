using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using System;
    using Data;
    using Den.Tools;
    using IO.Swagger.Model;
    using masterland.Wallet;

    public class UILandInfo : MonoBehaviour
    {
        [SerializeField] private Tab_Land _tab_Land;
        [SerializeField] private GameObject _loadingOb;
        [SerializeField] private GameObject _contentOb;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _leasingBtn;
        [SerializeField] private Button _registerResidentBtn;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TMP_InputField _addressText;
        [SerializeField] private GameObject _hasLeasingOb;
        [SerializeField] private GameObject _hasntLeasingOb;

        private LandData _landData;

        void Start()
        {
            _closeBtn.onClick.AddListener(Hide);
            _registerResidentBtn.onClick.AddListener(ResgisterOrUnregister);
        }

        public async void Show(string landAddress)
        {
            gameObject.SetActive(true);
            _loadingOb.gameObject.SetActive(true);
            _contentOb.SetActive(false);
            _landData = await WalletInteractor.Instance.GetLand(landAddress);
            _nameText.text = _landData.Name;
            _addressText.text = _landData.Id.ToShortAddress();
            _hasLeasingOb.SetActive(_landData.HasLeased);
            _hasntLeasingOb.SetActive(!_landData.HasLeased);

            UpdateRegisterBtnContent();   

            _registerResidentBtn.interactable = true;
            _loadingOb.gameObject.SetActive(false);
            _contentOb.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
            _landData = null;
        }

        public void UpdateRegisterBtnContent() {
            if(_landData == null)
                return;
            string currentLandId = _landData.Name.Split("#")[1];
            _registerResidentBtn.GetComponentInChildren<TextMeshProUGUI>().text = Data.Instance.ResidentLicense ==null || !(!string.IsNullOrEmpty(Data.Instance.ResidentLicense.Id) && Data.Instance.ResidentLicense.LandId == currentLandId)
                                                                     ? "Register Resident" : "Unregister Resident";
        }

        public async void ResgisterOrUnregister()
        {
            _registerResidentBtn.interactable = false;
            _registerResidentBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Processing...";

            string currentLandId = _landData.Name.Split("#")[1];
            var residentLicense = Data.Instance.ResidentLicense;

            bool isRegister =  Data.Instance.ResidentLicense == null || !(!string.IsNullOrEmpty(Data.Instance.ResidentLicense.Id) && Data.Instance.ResidentLicense.LandId == currentLandId);

            if (isRegister)
            {
                bool shouldUnregister = Data.Instance.ResidentLicense != null &&
                                    !string.IsNullOrEmpty(residentLicense.Id) &&
                                    residentLicense.LandId != currentLandId;
                if (shouldUnregister)
                {
                    Debug.Log("You must unregister current resident license");
                }
                else
                {
                    //register
                    ContractRespone contractRespone = await WalletInteractor.Instance.RegisterResidentLicense(Data.Instance.MasterData.Id, currentLandId);
                    if (!contractRespone.IsSucess)
                    {
                        Debug.Log("Can't Register");
                    }
                }
            }
            else
            {
                //unregister
                ContractRespone contractRespone = await WalletInteractor.Instance.UnregisterResidentLicense(Data.Instance.MasterData.Id);
                if (!contractRespone.IsSucess)
                {
                    Debug.Log("Can't Unregister");
                }
            }

            await Data.Instance.GetSelectedMaster();
            _tab_Land.SetupCurrentPanel();
            _registerResidentBtn.interactable = true;
             UpdateRegisterBtnContent();   
        }


    }
}
