using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

namespace masterland.Master
{
    public class Animation : MasterComponent
    {
        private Animator _animator;
        public NetworkAnimator NetworkAnimator;
        
        private int _animIDVertical;
        private int _animIDHorizontal;
        private int _animIDGrounded;
        private int _animIDSwordLM;
        private float _currentWeight;
        private float _smoothWeightTime =3f;

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
            _animIDSwordLM = Animator.StringToHash("sword");
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
        public void SetWeaponType(int typeId) 
        {
             _animator.SetBool(_animIDSwordLM, typeId == 1);
        }

        public void PlayAction(string id) 
        {
            _animator.Play(id, 2, 0);
        }

        public void PlayActionObserver(string id) {
            NetworkAnimator.Play(id, 2, 0);
        }

        public void PlayCrossFadeAction(string id) 
        {
            _animator.CrossFade(id, 0.2f, 2);
        }

    }
}
