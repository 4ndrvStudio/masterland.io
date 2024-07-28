using System.Collections;
using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using JetBrains.Annotations;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace masterland.Master
{
    public enum AttackType
    {
        Normal = 0,
        Jump = 1,
        Run = 2
    }
    public enum DodgeType
    {
        None,
        Front,
        Back,
        Left,
        Right
    }

    [Serializable]
    public class ComboAttack 
    {
        public int Id;
        public string Name;
        public List<string> AttackList;
    }

    public class Combat : MasterComponent
    {
        public int AttackIndex;
        public List<ComboAttack> ComboAttackList = new();
        public List<string> NormalAttackList;
        public string NormalJumpAttack;
        public string RunAttack;
        public string FastDodgeLeft;
        public string FastDodgeRight;
        public bool _avoidMultiAttack;
        public string DodgeLeft;
        public string DodgeRight;
        public string DodgeFront;
        public string DodgeBack;
        
        public Collider CurrentTargetLockOn;

        [SerializeField] private ComboAttack _currentCombo;

        public void Start() 
        {
            _currentCombo = ComboAttackList[UnityEngine.Random.Range(0, ComboAttackList.Count)];
        }

        public override void OnTick()
        {
            base.OnTick();
            if (IsOwner)
            {
                NormalAttackState();
                DodgeState();
            }

        }

        public void ResetCombat()
        {
            _master.Input.PlayAttack = false;
            AttackIndex = 0;
        }

        public void NormalAttackState()
        {
            if (!_master.State.IsAction && _master.Input.PlayAttack
                    && _master.Weapon.GetTypeId() != 0 && !_avoidMultiAttack)
            {
                _master.Input.PlayAttack = false; 
                if(_master.State.IsAction)
                   return;
                StartCoroutine(AvoidMultiAttack());
              
                float targetRotation;

                if(_master.Input.PlayLockOn) 
                    /* Mobile */
                    // Vector3 directionToTarget = _master.AimAssist.SelectedNearest.transform.position - transform.position;
                    // directionToTarget.y = 0;
                    // Quaternion targetRotationQuaternion = Quaternion.LookRotation(directionToTarget);
                    // targetRotation = targetRotationQuaternion.eulerAngles.y;
                    targetRotation = Mathf.Atan2(Camera.main.transform.forward.x, Camera.main.transform.forward.z) * Mathf.Rad2Deg;
                 else 
                    targetRotation = Mathf.Infinity;
                

                if (_master.State.IsGrounded)
                {
                    if (_master.Movement.RunSpeedTime > 0.8f)
                    {
                        ProcessAttack(1, AttackType.Run, targetRotation);
                    }
                    else
                    {                       
                        //random combo attack
                        if(AttackIndex >= _currentCombo.AttackList.Count) {
                            AttackIndex = 0;
                            _currentCombo = ComboAttackList[UnityEngine.Random.Range(0, ComboAttackList.Count)];
                        }
                        ProcessAttack(AttackIndex, AttackType.Normal, targetRotation);
                        AttackIndex = AttackIndex + 1;
                    }
                }
                else
                {
                    if (_master.Movement.JumpTime < 0.7f)
                        ProcessAttack(1, AttackType.Jump, targetRotation);
                }
            }
        }

        public void DodgeState() 
        {
            if (!_master.State.IsAction && _master.State.IsGrounded )
            {
               
                if( _master.Stats.MP.Value <50 && _master.Movement.RunSpeedTime < 0.9f) 
                {
                    _master.Input.PlayFastDodgeLeft = false;
                    _master.Input.PlayFastDodgeRight = false;
                     return;
                }
                 
                if (_master.Input.PlayFastDodgeLeft) {
                     _master.Input.PlayFastDodgeLeft = false;
                    ProcessFastDodge(0);
                    _master.Stats.TakeMP(50);
                }
                   
                if (_master.Input.PlayFastDodgeRight) {
                    _master.Input.PlayFastDodgeRight = false;
                    ProcessFastDodge(1);
                    _master.Stats.TakeMP(50);
                }
            }


            if (!_master.State.IsAction 
                && _master.Input.PlayDodge)
            {
                _master.Input.PlayDodge = false;

                if (_master.Input.MoveDirection == Vector2.zero || _master.Stats.MP.Value <45 )  
                    return;
                
                DodgeType dodgeType = DodgeType.None;

                if (_master.Input.MoveDirection.y > 0)
                    dodgeType = DodgeType.Front;

                if (_master.Input.MoveDirection.y < 0)
                    dodgeType = DodgeType.Back;

                if (_master.Input.MoveDirection.y == 0 && _master.Input.MoveDirection.x > 0)
                    dodgeType = DodgeType.Right;
                if (_master.Input.MoveDirection.y == 0 && _master.Input.MoveDirection.x < 0)
                    dodgeType = DodgeType.Left;

                Vector3 inputDirection = new Vector3(_master.Input.MoveDirection.x, 0.0f, _master.Input.MoveDirection.y).normalized;
                float rotateFactor = dodgeType == DodgeType.Back ? 180 :0;
                float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + transform.eulerAngles.y + rotateFactor;
                ProcessDodge(dodgeType, targetRotation);
            }
        }

        private IEnumerator AvoidMultiAttack()
        {
            _avoidMultiAttack = true;
            yield return new WaitForSeconds(0.3f);
            _avoidMultiAttack = false;
        }
        
        public void ProcessAttack(int index, AttackType type, float targetRotation = Mathf.Infinity)
        {
            if(targetRotation != Mathf.Infinity)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, targetRotation, 0.0f), 1);
            
            Debug.Log(_currentCombo.AttackList[index]);
            string attackTarget = string.Empty;
            switch (type)
            {
                case AttackType.Normal:
                    attackTarget = _currentCombo.AttackList[index];
                    break;
                case AttackType.Run:
                    attackTarget = RunAttack;
                    break;
                case AttackType.Jump:
                    attackTarget = NormalJumpAttack;
                    break;
            }

            if (string.IsNullOrEmpty(attackTarget))
                return;

            _master.Animation.Server_PlayAction(attackTarget);
        }

        public void ProcessFastDodge(int index) => _master.Animation.Server_PlayAction(index == 1 ? FastDodgeRight : FastDodgeLeft);

        public void ProcessDodge(DodgeType dodgeType, float targetRotation)
        {
            if(dodgeType != DodgeType.Left && dodgeType != DodgeType.Right)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, targetRotation, 0.0f), 1f);
          
            string targetDodge = string.Empty;
            switch (dodgeType)
            {
                case DodgeType.Front:
                    targetDodge = DodgeFront;
                    break;
                case DodgeType.Back:
                    targetDodge = DodgeBack;
                    break;
                case DodgeType.Left:
                    targetDodge = DodgeLeft;
                    break;
                case DodgeType.Right:
                    targetDodge = DodgeRight;
                    break;

            }
            _master.Animation.Server_PlayAction(targetDodge);
            _master.Stats.TakeMP(45);
        }



    }
}
