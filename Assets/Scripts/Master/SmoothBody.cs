using UnityEngine;

namespace masterland
{
    public class SmoothBody : MonoBehaviour
    {
        public Transform parentTransform;
        public float smoothSpeed = 0.125f;


        private Vector3 lastPosition;
        private Quaternion lastRotation;

        void Start()
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        void LateUpdate()
        {
            Vector3 desiredPosition = parentTransform.position;
            Quaternion desiredRotation = parentTransform.rotation;

            Vector3 smoothedPosition = Vector3.Lerp(lastPosition, desiredPosition, smoothSpeed);
            Quaternion smoothedRotation = Quaternion.Lerp(lastRotation, desiredRotation, smoothSpeed);

            transform.position = smoothedPosition;
            transform.rotation = smoothedRotation;

            lastPosition = smoothedPosition;
            lastRotation = smoothedRotation;
        }
    }
}
