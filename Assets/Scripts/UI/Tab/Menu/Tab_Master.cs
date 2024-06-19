using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using Cysharp.Threading.Tasks;
    using Data;
    using masterland.Wallet;
    using TMPro;

    public class Tab_Master : UITab
    {
    
        [SerializeField] private Button _mintBtn;
        [SerializeField] private GameObject _mintIcon;
        [SerializeField] private GameObject _mintLoadingIcon;
        public List<MasterSlot> MasterSlots;
        [SerializeField] private GameObject _masterOb;
        [SerializeField] private GameObject _container;
       
        [SerializeField] private bool _isLoadingFirst;
        [SerializeField] private GameObject _loadingOb;

        //current master panel 
        [SerializeField] private GameObject _currentMasterPanel;
        [SerializeField] private TextMeshProUGUI _currentMasterNameText;
        [SerializeField] private TMP_InputField _currentMasterAddressText;

        private void Start()
        {
            _mintBtn.onClick.AddListener(Mint);
        }
        public override async void Show()
        {
            base.Show();
            _isLoadingFirst = true;
            _loadingOb.SetActive(true);
            await Data.Instance.GetSelectedMaster();
            SetupCurrentPanel();
            StartCoroutine(GetMasters());
           
        }

        public override void Hide()
        {
            base.Hide();
            MasterSlots.ForEach(item => Destroy(item.gameObject));
            MasterSlots.Clear();
            _container.GetComponent<RectTransform>().pivot = new Vector2(0.5f,1);
        }
        public async void Mint()
        {
            _mintBtn.interactable = false;
            _mintIcon.SetActive(false);
            _mintLoadingIcon.SetActive(true);
            await WalletInteractor.Instance.MintMaster();
            _mintBtn.interactable = true;
            _mintIcon.SetActive(true);
            _mintLoadingIcon.SetActive(false);
        }

        IEnumerator GetMasters()
        {
            while (true)
            {
                OnMasterListUpdate();
                yield return new WaitForSeconds(3f);
            }
        }

        public void SetupCurrentPanel() {
            _currentMasterPanel.SetActive(Data.Instance.MasterData != null && !string.IsNullOrEmpty(Data.Instance.MasterData.Id));
            _currentMasterNameText.text = Data.Instance.MasterData.Name;
            _currentMasterAddressText.text = Data.Instance.MasterData.Id.ToShortAddress();
        }


        public async void OnMasterListUpdate()
        {
            Data.Instance.OwnedMasters = await WalletInteractor.Instance.GetMasters();
           
            if(Data.Instance.OwnedMasters.Count != MasterSlots.Count) {
                    await Data.Instance.GetSelectedMaster();
                    SetupCurrentPanel();
            }

            if (_isLoadingFirst)
            {
                _loadingOb.SetActive(false);
                _isLoadingFirst = false;
            }

            foreach (MasterData master in Data.Instance.OwnedMasters)
            {
                int index = MasterSlots.FindIndex(item => master.Id == item.MasterData.Id);
                if (index == -1)
                {
                    var masterSlot = Instantiate(_masterOb, _container.transform);
                    masterSlot.SetActive(true);
                    var script = masterSlot.GetComponent<MasterSlot>();
                    script.Setup(master,this);
                    MasterSlots.Add(script);
                }
            }

            List<MasterSlot> slotsToRemove = new List<MasterSlot>();
            foreach (MasterSlot masterSlot in MasterSlots)
            {
    
                int index = Data.Instance.OwnedMasters.FindIndex(item => item.Id == masterSlot.MasterData.Id);
                if (index == -1)
                {
                    slotsToRemove.Add(masterSlot);
                }
            }

            foreach (MasterSlot masterSlot in slotsToRemove)
            {
                MasterSlots.Remove(masterSlot);
                Destroy(masterSlot.gameObject);
            }

            _container.GetComponent<RectTransform>().pivot = new Vector2(MasterSlots.Count >2 ? 0: 0.5f,1);
        }


    }
}
