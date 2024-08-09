using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace masterland.UI
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Cysharp.Threading.Tasks;
    using Data;
    using masterland.Wallet;
    using Suinet.Rpc;
    using Suinet.Rpc.Types;
    using UnityEngine.Events;

    public class ConfirmTxData
    {
        public string Title;
        public string Tx;
        public string Gas;
        public string Fee;
        public string BalanceChange;
        public UniTaskCompletionSource<ContractRespone> Tcs;
    }

    public class Popup_SuiWallet : UIPopup
    {
        [Header("Panel")]
        [SerializeField] private GameObject _walletPanel;
        [SerializeField] private GameObject _confirmPanel;

        [Header("Wallet")]
        [SerializeField] private TextMeshProUGUI _addressText;
        [SerializeField] private TextMeshProUGUI _balanceValueText;
        [SerializeField] private Button _copyBtn;
        [SerializeField] private Button _logoutBtn;
        [SerializeField] private Button _requestTokenBtn;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _dimmedBtn;
        [SerializeField] private GameObject _copyOb;
        [SerializeField] private GameObject _copiedOb;

        [Header("Confirm")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _feeText;
        [SerializeField] private TextMeshProUGUI _gasText;
        [SerializeField] private TextMeshProUGUI _sumText;
        [SerializeField] private Button _confirmBtn;
        [SerializeField] private Button _rejectBtn;
        [SerializeField] private GameObject _confirmTextOb;
        [SerializeField] private GameObject _loadingOb;

        private bool _isConfirmTx;
        private ConfirmTxData _confirmTxData;

        void Start()
        {
            _copyBtn.onClick.AddListener(()=> StartCoroutine(OnCopy()));
            _logoutBtn.onClick.AddListener(()=> {
                WalletInGame.Logout();
                Hide();
            });
            _requestTokenBtn.onClick.AddListener(OnRequestToken);
            _closeBtn.onClick.AddListener(Hide);
            _dimmedBtn.onClick.AddListener(() => {
                if(!_isConfirmTx)
                    Hide();
            });

            _confirmBtn.onClick.AddListener(ExecuteTx);
            _rejectBtn.onClick.AddListener(() => ReturnResult(false,null, "User Reject"));
        }

        public override void Show(Dictionary<string, object> customProperties = null)
        {
            base.Show(customProperties);
            _isConfirmTx = customProperties["isConfirmTx"] is bool isConfirm && isConfirm;
            _walletPanel.SetActive(!_isConfirmTx);
            _confirmPanel.SetActive(_isConfirmTx);
            
            if(!_isConfirmTx) 
            {
                _addressText.text = SuiWallet.GetActiveAddress().ToShortAddress();
                StartCoroutine(FetchData());
            } 
            else 
            {
                ConfirmTxData confirmTxData = customProperties["confirmTxData"] as ConfirmTxData;
                _confirmTxData = confirmTxData;
                _titleText.text =confirmTxData.Title;
                _gasText.text = confirmTxData.Gas;
                _feeText.text = confirmTxData.Fee;
                _sumText.text = confirmTxData.BalanceChange;
            }
        }

        public override void Hide()
        {
            base.Hide();
            SetCopyState(false);
            StopAllCoroutines();
        }
        public void SetCopyState(bool iscopying) 
        {
            _copyBtn.interactable = !iscopying;
            _copyOb.gameObject.SetActive(!iscopying);
            _copiedOb.gameObject.SetActive(iscopying);
        }

        IEnumerator OnCopy() 
        {   
            SetCopyState(true);
            UniClipboard.SetText(SuiWallet.GetActiveAddress());
            yield return new WaitForSeconds(1f);
            SetCopyState(false);
        }

        public async void OnRequestToken() 
        {
            _requestTokenBtn.interactable = false;
            await SuiAirdrop.RequestAirdrop(SuiWallet.GetActiveAddress());
            _requestTokenBtn.interactable = true;
        }

        IEnumerator FetchData() 
        {
            while (true && gameObject.activeSelf)
            {
                UpdateBalance();
                yield return new WaitForSeconds(2);
            }
        }
        async void UpdateBalance() {
            if(SuiWallet.GetActiveAddress() != "0x")
                _balanceValueText.text = await WalletInGame.GetBalance();
        }

        public void ExecutingUI(bool isExecuting) {
            _confirmTextOb.SetActive(!isExecuting);
            _confirmBtn.interactable = !isExecuting;
            _rejectBtn.interactable = !isExecuting;
            _loadingOb.SetActive(isExecuting);
        }

        public async void ExecuteTx() 
        {
            ExecutingUI(true);
            var rpcResult = await WalletInGame.Execute(_confirmTxData.Tx);

            ExecutingUI(false);
            if(rpcResult.IsSuccess) {
                ReturnResult(true, rpcResult);
            } else 
                ReturnResult(false, null, rpcResult.ErrorMessage);
        }

        public void ReturnResult(bool isSuccess, object data, string message = null) 
        {
                ContractRespone contractRespone = new ContractRespone() {
                    IsSuccess = isSuccess,
                    Data = data,
                    Message = message,
                };
                _confirmTxData.Tcs.TrySetResult(contractRespone);
                Hide();
        }
        


    }
}
