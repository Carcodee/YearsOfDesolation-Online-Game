using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IKPn.Example.Example2D
{
    public class SpriteExample : MonoBehaviour
    {
        public Transform nextBone;
        public float depth = 0;
        public bool rotate = false;

        internal void UpdateState()
        {
            if (rotate)
            {
                transform.rotation = Quaternion.identity;
                transform.Rotate(Vector3.forward, Time.timeSinceLevelLoad * 100);
            }
            else
            {
                var _nextBoneDir = nextBone.position - transform.position;
                _nextBoneDir.z = transform.position.z;
                transform.LookAt(transform.position + Vector3.forward, _nextBoneDir);
            }

            transform.localPosition = Vector3.zero;
            transform.position += Vector3.forward * depth;
        }
    }
}