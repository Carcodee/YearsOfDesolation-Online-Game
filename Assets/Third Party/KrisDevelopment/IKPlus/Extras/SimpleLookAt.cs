using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn.Extras
{
    [AddComponentMenu("IK Plus/Simple Look-At")]
    public class SimpleLookAt : MonoBehaviour
    {
        public Transform target;
        public bool vertical = true;

        void Update()
        {
            transform.LookAt(vertical ? target.position : new Vector3(target.position.x, transform.position.y, target.position.z));
        }
    }
}