using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland.Master
{
    public class State : MasterComponent
    {
        public bool IsAction;
        public bool IsActionKeepSpeed;
        public bool IsHardAction;
        public bool IsDodge;
        public bool IsGetHit;
        public bool IsGrounded;
        public bool IsGuard;
        public bool IsTired;
        public bool IsBlock;
        
        
        public override void OnTick() 
        {
            base.OnTick();
            IsGetHit =_master.Animator.GetCurrentAnimatorStateInfo(2).IsTag("GetHitState");
            IsGuard = _master.Animator.GetCurrentAnimatorStateInfo(2).IsTag("GuardState");
            IsHardAction = _master.Animator.GetCurrentAnimatorStateInfo(2).IsTag("HardAction") || IsGetHit || IsGuard;
            IsDodge =_master.Animator.GetCurrentAnimatorStateInfo(2).IsTag("Dodge");
            IsAction = _master.Animator.GetCurrentAnimatorStateInfo(2).IsTag("Action")
                        || IsHardAction
                        || IsDodge;

            IsActionKeepSpeed = _master.Animator.GetCurrentAnimatorStateInfo(2).IsName(_master.Combat.FastDodgeLeft)
                                || _master.Animator.GetCurrentAnimatorStateInfo(2).IsName(_master.Combat.FastDodgeRight);

            IsGrounded = _master.Movement.Grounded;           
        }

    }
       
    
}
