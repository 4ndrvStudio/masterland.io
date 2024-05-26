using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace masterland
{
    public class CameraManager : Singleton<CameraManager>
    {
        public Camera MainCamara;
        [SerializeField] private CinemachineFreeLook _cineCam;
        [SerializeField] private CinemachineVirtualCamera _lockOnCam;
        [SerializeField] private TargetLookPoint _targetLookPoint;
        [SerializeField] private TargetLookPoint _targetLockPoint;
        
        public Transform TargetLock;

        public void Awake() {
            _cineCam.gameObject.SetActive(false);
            _lockOnCam.gameObject.SetActive(false);

        }

        public void SetActive(bool isActive = true) 
        {
            _cineCam.gameObject.SetActive(isActive);
        }

        public void SetLockOn(bool isActive, Collider target = null) 
        {
            if(target !=null)
                _targetLockPoint.TargetFollow = target.transform;
            _lockOnCam.gameObject.SetActive(isActive);
            _cineCam.gameObject.SetActive(!isActive);
        } 

        public void Setup(Transform follow) {
            _targetLookPoint.TargetFollow = follow;
            _cineCam.gameObject.SetActive(true);
        }

        public float GetAngle() => MainCamara.transform.eulerAngles.y;
    }
}
