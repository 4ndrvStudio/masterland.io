using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland.Master
{
    public class AimAssist : MasterComponent
    {
        [SerializeField] private LayerMask _enemyMask;
        [SerializeField] private float _zone = 20f;
        [SerializeField] private float _nearestTemp;
        [SerializeField] private Transform _aimArrow;
        private Transform _aimArrowClone;
        public Collider SelectedNearest;
        public GameObject UIContainer;

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (_aimArrowClone != null)
                _aimArrowClone.gameObject.SetActive(_master.Input.PlayLockOn);

        }

        public Collider GetTargetToAim()
        {
            if (!IsOwner)
                return null;
     
            var nearestToAttack = new List<Collider>(Physics.OverlapSphere(transform.position, _zone, _enemyMask));

            SelectedNearest = null;
            _nearestTemp = Mathf.Infinity;

            if (nearestToAttack.Count > 0)
            {

                foreach (var targetAttack in nearestToAttack)
                {
                    if (targetAttack.GetComponent<Master>().ObjectId != this.ObjectId)
                    {
                       
                        float distance = Vector3.Distance(targetAttack.transform.position, transform.position);
                        if (distance < _nearestTemp)
                        {
                            SelectedNearest = targetAttack;
                            _nearestTemp = distance;
                        }
                    }
                }
            }


            if (SelectedNearest != null)
            {
                if (_aimArrowClone == null)
                    _aimArrowClone = Instantiate(_aimArrow);

                Transform uiTransform = SelectedNearest.GetComponent<Master>().AimAssist.UIContainer.transform;
                _aimArrowClone.SetParent(uiTransform);
                _aimArrowClone.transform.localPosition = Vector3.zero;
                Quaternion rotation = Quaternion.Euler(0, 0, 180);
                _aimArrowClone.transform.localRotation =rotation;
            }
            return SelectedNearest;
        }
    }
}
