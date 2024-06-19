using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

namespace masterland.Master
{
    public enum JumpState 
    {
        Start, 
        Loop,
        End,
    }

    public class Animation : MasterComponent
    {
        private Animator _animator;
        public NetworkAnimator NetworkAnimator;
        
        private int _animIDVertical;
        private int _animIDHorizontal;
        private int _animIDGrounded;
        private int _animIDSliding;
        private int _animIDSwordLM;
        private float _currentWeight;
        private float _smoothWeightTime =3f;

        private int _animIDJumpRunningStart;
        private int _animIDJumpRunningLoop;
        private int _animIDJumpRunningEnd;


        public bool HasWeapon 
        {
            set {
                _animator.SetLayerWeight(2,value? 0.75f: 0);
            }
        }
        
        public override void Setup(Master master)
        {
            base.Setup(master);
            _animator = master.Animator;
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            _animIDVertical = Animator.StringToHash("vertical");
            _animIDHorizontal = Animator.StringToHash("horizontal");
            _animIDGrounded = Animator.StringToHash("grounded");
            _animIDSliding = Animator.StringToHash("sliding");
            _animIDSwordLM = Animator.StringToHash("sword");

            _animIDJumpRunningStart = Animator.StringToHash("JumpRunningStart");
            _animIDJumpRunningLoop = Animator.StringToHash("JumpRunningLoop");
            _animIDJumpRunningEnd = Animator.StringToHash("JumpRunningEnd");
        }

        public void LocoMovement(float blend) 
        {
            if(_animator == null) return;
            _currentWeight = Mathf.Lerp(_currentWeight,0,_smoothWeightTime *(float) TimeManager.TickDelta);
            _animator.SetLayerWeight(1,_currentWeight);
            _animator.SetFloat(_animIDVertical, blend,0.05f,(float) TimeManager.TickDelta);
        }
        public void LockOnMovement(float vertical, float horizontal) 
        {
            if(_animator == null) return;
            _currentWeight = Mathf.Lerp(_currentWeight,1,_smoothWeightTime *(float) TimeManager.TickDelta);
            _animator.SetLayerWeight(1,_currentWeight);
            _animator.SetFloat(_animIDVertical, vertical,0.001f,(float) TimeManager.TickDelta);
            _animator.SetFloat(_animIDHorizontal, horizontal,0.001f,(float) TimeManager.TickDelta);
        }

        public void SetGrounded(bool isActive) {
            _animator.SetBool(_animIDGrounded, isActive);
        }

        
        public void SetJumpState(JumpState jumpState) 
        {
            Debug.Log(jumpState.ToString());
            _animator.SetBool(_animIDJumpRunningStart, jumpState == JumpState.Start);
            _animator.SetBool(_animIDJumpRunningLoop, jumpState == JumpState.Loop);
            _animator.SetBool(_animIDJumpRunningEnd, jumpState == JumpState.End);
        }

        public void SetJump(bool isActive) 
        {
            _animator.SetBool("jump", isActive);

        }


        public void SetSliding(bool isActive) {
            _animator.SetBool(_animIDSliding, isActive);
        }

        public void SetWeaponType(int typeId) 
        {
             _animator.SetBool(_animIDSwordLM, typeId == 1);
        }

        
        public void PlayAction(string id) 
        {
            if(_animator.GetCurrentAnimatorStateInfo(2).IsName(id) && !_master.State.IsGetHit)
                return;
            _animator.Play(id, 2, 0);
        }

        [ServerRpc]
        public void Server_PlayAction(string id) {
        
            PlayAction(id);
            Observers_PlayAction(id);
        }

        [ObserversRpc(ExcludeServer = true)]
        public void Observers_PlayAction(string id) {
            PlayAction(id);
        }


     

    }
}
