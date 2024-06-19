using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.CodeGenerating;
using FishNet.Object.Prediction;
using UnityEngine.UI;

namespace masterland.Master
{
    public class Stats : MasterComponent
    {
        [AllowMutableSyncType]
        public SyncVar<byte> HP = new SyncVar<byte>(0, new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        [ServerRpc(RunLocally = true)]
        public void SetHp(byte value) => HP.Value = value;

        [AllowMutableSyncType]
        public SyncVar<byte> MP = new SyncVar<byte>(0, new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

        public float CurrentMP;
        public void SetMP(float value)
        {
            MP.Value = (byte)value;
        }
        [ServerRpc]
        public void TakeMP(float value) 
        {
            if(CurrentMP>=45)
                CurrentMP-= value;
        }

        private int _initMP = 100;

        [Header("Refs")]
        [SerializeField] private Image _manaInnerImg;
        [SerializeField] private Image _manaEffectImg;
        [SerializeField] private bool _isRequireManaEffect;

        public override void Setup(Master master)
        {
            base.Setup(master);
            CurrentMP = _initMP;
            _manaEffectImg.gameObject.SetActive(IsOwner);

            MP.OnChange += OnMPChange;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            MP.OnChange -= OnMPChange;
        }

        public void OnMPChange(byte pre, byte next, bool asServer)
        {
            if (asServer)
            {
                if (next <= 1)
                {
                    _master.State.IsTired = true;
                    SetTired_Rpc(true);
                }


                if (_master.State.IsTired && next > 50f)
                {
                    _master.State.IsTired = false;
                    SetTired_Rpc(false);
                }
            }
            else
            {
                if (Mathf.Abs(pre - next) > 30)
                {
                    StartCoroutine(IEManaEffect(pre));
                }
            }
        }

        IEnumerator IEManaEffect (byte pre) {
            _isRequireManaEffect = true;
            _manaEffectImg.fillAmount = (float) pre / (float) _initMP;
            _manaEffectImg.enabled =true;
            yield return new WaitForSeconds(0.4f);
            _isRequireManaEffect = false;
        }

        [ObserversRpc(ExcludeServer = true)]
        public void SetTired_Rpc(bool isTired)
        {
            _master.State.IsTired = isTired;
        }


        public override void OnTick()
        {
            base.OnTick();
            if (IsOwner)
            {
                float mp = (float)MP.Value / (float)_initMP;
                _manaInnerImg.fillAmount = Mathf.Lerp(_manaInnerImg.fillAmount, mp, 10 * (float)TimeManager.TickDelta);
                _manaInnerImg.enabled = !_master.State.IsTired;
                if (!_isRequireManaEffect)
                {
                    _manaEffectImg.enabled = _master.Reconcile.ReplicateData.Sprint || _master.State.IsTired;
                    float lerpSpeed = _master.State.IsTired ? 10f : 2f;
                    _manaEffectImg.fillAmount = Mathf.Lerp(_manaEffectImg.fillAmount, mp, lerpSpeed * (float)TimeManager.TickDelta);

                }
            }

            if (!IsServerInitialized)
                return;

            bool isSprint = _master.Reconcile.ReplicateData.Sprint;
            bool isLockOn = _master.Reconcile.ReplicateData.LockOn;

            float factor = ((float)TimeManager.TickDelta * 25f);
            CurrentMP = isSprint && !_master.State.IsAction && !_master.State.IsTired ? CurrentMP -= factor
                                    : CurrentMP += factor;
            CurrentMP = Mathf.Clamp(CurrentMP, 1f, _initMP);

            SetMP(CurrentMP);
        }
    }
}
