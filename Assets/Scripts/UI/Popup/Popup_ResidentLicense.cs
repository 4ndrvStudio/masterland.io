using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using System;
    using Data;
    using TMPro;

    public class Popup_ResidentLicense : UIPopup
    {
        [Header("Panel")]
        [SerializeField] private GameObject _loadingOb;
        [SerializeField] private GameObject _contentOb;
        [Header("Display")]
        [SerializeField] private List<Button> _closeBtnList = new();
        [SerializeField] private TextMeshProUGUI _landNameText;
        [SerializeField] private TextMeshProUGUI _woodBalanceText;
        [SerializeField] private TextMeshProUGUI _stoneBalanceText;
        [SerializeField] private TextMeshProUGUI _woodPerDayText;
        [SerializeField] private TextMeshProUGUI _stonePerDayText;
        [SerializeField] private TextMeshProUGUI _canMintWoodText;
        [SerializeField] private TextMeshProUGUI _canMintStoneText;

        private float _timer;
        private float _updateInterval = 1f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _closeBtnList.ForEach(item => item.onClick.AddListener(Hide));
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(Data.Instance.ResidentLicense == null || string.IsNullOrEmpty(Data.Instance.ResidentLicense.Id))
                return;

            _timer += Time.fixedDeltaTime;

            if (_timer >= _updateInterval)
            {
                _timer = 0f; // Reset the timer

                  
               TimeSpan woodRemainingTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(Data.Instance.ResidentLicense.NextTimeMintWood)/1000).UtcDateTime - DateTimeOffset.UtcNow;
               TimeSpan stoneRemainingTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(Data.Instance.ResidentLicense.NextTimeMintStone)/1000).UtcDateTime - DateTimeOffset.UtcNow;
                if(woodRemainingTime.Milliseconds > 0)
                    _canMintWoodText.text = $"Can mint in {woodRemainingTime.Hours}:{woodRemainingTime.Minutes}:{woodRemainingTime.Seconds}";
                else 
                {
                    if(int.Parse(Data.Instance.ResidentLicense.WoodMintedPerDay)==int.Parse(Data.Instance.ResidentLicense.WoodLimitedPerDay)) {
                        _woodPerDayText.text = "0/"+Data.Instance.ResidentLicense.WoodLimitedPerDay;
                    }
                }

                if(stoneRemainingTime.Milliseconds > 0) 
                    _canMintStoneText.text = $"Can mint in {stoneRemainingTime.Hours}:{stoneRemainingTime.Minutes}:{stoneRemainingTime.Seconds}";
                else 
                {
                    if(int.Parse(Data.Instance.ResidentLicense.StoneMintedPerDay)==int.Parse(Data.Instance.ResidentLicense.StoneLimitedPerDay)) 
                        _stonePerDayText.text = "0/"+Data.Instance.ResidentLicense.StoneLimitedPerDay;
                }
                
                if (!_canMintWoodText.gameObject.activeSelf)
                    _canMintWoodText.gameObject.SetActive(woodRemainingTime.Milliseconds > 0 && Data.Instance.ResidentLicense.WoodMintedPerDay == Data.Instance.ResidentLicense.WoodLimitedPerDay);
                if (!_canMintStoneText.gameObject.activeSelf)
                    _canMintStoneText.gameObject.SetActive(stoneRemainingTime.Milliseconds > 0 && Data.Instance.ResidentLicense.StoneMintedPerDay == Data.Instance.ResidentLicense.StoneLimitedPerDay);
            }
        }

        public override void Show(Dictionary<string, object> customProperties = null)
        {
            base.Show(customProperties);
            FetchData();
        }

        public async void FetchData()
        {
            _loadingOb.SetActive(true);
            _contentOb.SetActive(false);
            await Data.Instance.GetSelectedMaster();
            _landNameText.text = "Land #" + Data.Instance.ResidentLicense.LandId;
            _woodBalanceText.text = Data.Instance.ResidentLicense.WoodBalance;
            _stoneBalanceText.text = Data.Instance.ResidentLicense.StoneBalance;
            _woodPerDayText.text = $"{Data.Instance.ResidentLicense.WoodMintedPerDay}/{Data.Instance.ResidentLicense.WoodLimitedPerDay}";
            _stonePerDayText.text = $"{Data.Instance.ResidentLicense.StoneMintedPerDay}/{Data.Instance.ResidentLicense.StoneLimitedPerDay}";

            _loadingOb.SetActive(false);
            _contentOb.SetActive(true);

        }

    }
}
