using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace masterland
{
    public class TargetLookPoint : MonoBehaviour
    {
        public Transform TargetFollow;

        // Update is called once per frame
        void Update()
        {
            if(TargetFollow != null) {
                transform.position = Vector3.Lerp(transform.position, TargetFollow.transform.position + Vector3.up*1.2f , 30f * Time.deltaTime);
            }
        }
    }
}
