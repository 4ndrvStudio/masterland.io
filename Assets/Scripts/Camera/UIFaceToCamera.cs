using UnityEngine;

namespace masterland.UI
{
     public class FaceToCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            Transform mainCamara = CameraManager.Instance.MainCamara.transform;
            transform.LookAt(transform.position + mainCamara.rotation * Vector3.forward, mainCamara.rotation * Vector3.up);
        }
    }
}
