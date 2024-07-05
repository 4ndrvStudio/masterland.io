using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace masterland.Master
{
    using System;
    using FishNet.Component.Prediction;
    using FishNet.Object;
    using Weapon;

    public enum SideHitType
    {
        Front,
        Back
    }
    public class Damageable : MasterComponent
    {
        [SerializeField] private string frontAnim;
        [SerializeField] private string backAnim;
        [SerializeField] private GameObject _guardSpark;

        [SerializeField] NetworkCollision _networkCollision;
        
        private bool _avoidTwiceHit;

        public void Start() 
        {
            _networkCollision.OnEnter += HandleWeaponCollide;
        }

        public override void OnDestroy() 
        {
            base.OnDestroy();
            if(_networkCollision != null) {
                _networkCollision.OnEnter -= HandleWeaponCollide;
            }
        }

        public void HandleWeaponCollide(Collider other)
        {
            if (other.tag != "Weapon" || !IsServerInitialized || _avoidTwiceHit)
                return;


            if (other.TryGetComponent<Weapon>(out Weapon weapon))
            {
                if(weapon.MasterOwner == null)
                    return;

                if (weapon.MasterOwner.OwnerId == _master.OwnerId  || _master.State.IsGuard )
                    return;


                if (_master.Reconcile.ReplicateData.LockOn && !_master.State.IsAction && _master.Stats.MP.Value > 50f)
                {
                    _master.Stats.CurrentMP -= 60f;
                    Block(weapon);
                }
                else
                {
                    GetAttack(weapon);
                }

                StartCoroutine(IEAvoidTwiceHit());
            }
        }

        IEnumerator IEAvoidTwiceHit() 
        {
            _avoidTwiceHit = true;
            yield return new WaitForSeconds(0.05f);
            _avoidTwiceHit = false;
        }


        public void Block(Weapon weapon)
        {
            Vector3 targetPosition = transform.position - transform.forward * 2f;
            Vector3 direction = weapon.MasterOwner.transform.position - transform.position;
            _master.AnimatorHook.WeaponColliderEnable =false;
            //RotatePlayer(direction, SideHitType.Front);
            Rpc_PlayBlock(direction);
            _master.Animation.PlayAction("guard");
            _master.Animation.Observers_PlayAction("guard");
            
        }

        [ObserversRpc(ExcludeServer = true)]
        public void Rpc_PlayBlock(Vector3 direction)
        {
            if (IsOwner)
            {
                _master.Movement.Controller.enabled = false;
                _master.AnimatorHook.WeaponColliderEnable =false;
                RotatePlayer(direction, SideHitType.Front);
                Vector3 targetPosition = transform.position - transform.forward * 2f;
                StartCoroutine(MoveToPosition(targetPosition, 0.2f, () =>
                                {
                                    _master.Movement.Controller.enabled = true;
                                }));                
            }
            //_master.Animation.PlayAction("guard");
            _master.Audio.PlayOneShot(AudioType.SwordHit);
            var spark = Instantiate(_guardSpark);
            spark.transform.position = _guardSpark.transform.position;
            spark.transform.rotation = _guardSpark.transform.rotation;
            spark.gameObject.SetActive(true);
            Destroy(spark, 1f);
        }

        public void GetAttack(Weapon weapon)
        {
            Vector3 direction = weapon.MasterOwner.transform.position - transform.position;
            Vector3 forward = transform.forward;
            float angle = Vector3.Angle(forward, direction);
            SideHitType sideHitType = angle < 280f ? SideHitType.Front : SideHitType.Back;
            string target = sideHitType == SideHitType.Front ? frontAnim : backAnim;
            _master.AnimatorHook.WeaponColliderEnable =false;
            Rpc_PlayGetHit(direction, sideHitType);
            //RotatePlayer(direction, sideHitType);
            _master.Animation.PlayAction(target);
            _master.Animation.Observers_PlayAction(target);
        }

        public void RotatePlayer(Vector3 direction, SideHitType sideHitType)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            if (sideHitType == SideHitType.Front)
                transform.rotation = targetRotation;
            else
                transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
        }

        [ObserversRpc(ExcludeServer = true)]
        public void Rpc_PlayGetHit(Vector3 direction, SideHitType sideHitType)
        {
      
            _master.AnimatorHook.WeaponColliderEnable =false;
            string target = sideHitType == SideHitType.Front ? frontAnim : backAnim;
            RotatePlayer(direction, sideHitType);
            //_master.Animation.PlayAction(target);
            _master.Audio.PlayOneShot(AudioType.GetHit);
        }


        private IEnumerator MoveToPosition(Vector3 targetPosition, float duration, System.Action onComplete)
        {
            float elapsedTime = 1f;
            Vector3 startPosition = transform.position;

            while (elapsedTime < duration)
            {
                Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                if (!IsCollidingWithObstacles(newPosition))
                {
                    transform.position = newPosition;
                }
                elapsedTime += (float)TimeManager.TickDelta;
                yield return null;
            }

            transform.position = targetPosition;

            onComplete?.Invoke();
        }

        private bool IsCollidingWithObstacles(Vector3 position)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
            foreach (Collider collider in colliders)
            {
                if (collider == GetComponent<Collider>())
                    continue;

                return true;
            }
            return false;
        }



    }
}
