using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn.Extras
{
    public class SimpleRotate : MonoBehaviour
    {
        [SerializeField] private Vector3 vector = Vector3.up;
        
        void Update()
        {
            transform.Rotate(vector * Time.deltaTime);
        }
    }
}
