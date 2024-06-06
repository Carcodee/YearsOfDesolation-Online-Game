using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Limb : FIK_ProcessorBase
    {
        [System.Serializable]
        public class IKBone : FIK_IKBoneBase
        {
            [SerializeField] private Quaternion targetToLocalSpace;
            [SerializeField] private Vector3 defaultLocalPoleNormal;

            public Vector3 right { get; private set; }
            public Vector3 up { get; private set; }
            public Vector3 forward { get; private set; }

            public Vector3 srcPosition { get; private set; }
            public Quaternion srcRotation { get; private set; }

            public IKBone(Transform t) : base(t) { }

            public void Init(Transform root, Vector3 childPosition, Vector3 orientationNormal)
            {
                RefreshOrientations(childPosition, orientationNormal);

                sqrMagn = (childPosition - transform.position).sqrMagnitude;
                LastKeyLocalRotation = transform.localRotation;

                right = transform.InverseTransformDirection(root.right);
                up = transform.InverseTransformDirection(root.up);
                forward = transform.InverseTransformDirection(root.forward);

                CaptureSourceAnimation();
            }

            public void RefreshOrientations(Vector3 childPosition, Vector3 orientationNormal)
            {
                Quaternion defaultTargetRotation = Quaternion.LookRotation(childPosition - transform.position, orientationNormal);
                targetToLocalSpace = RotationToLocal(transform.rotation, defaultTargetRotation);
                defaultLocalPoleNormal = Quaternion.Inverse(transform.rotation) * orientationNormal;
            }

            public void CaptureSourceAnimation()
            {
                srcPosition = transform.position;
                srcRotation = transform.rotation;
            }

            public static Quaternion RotationToLocal(Quaternion parent, Quaternion rotation)
            { return Quaternion.Inverse(Quaternion.Inverse(parent) * rotation); }

            public Quaternion GetRotation(Vector3 direction, Vector3 orientationNormal)
            { return Quaternion.LookRotation(direction, orientationNormal) * targetToLocalSpace; }

            public Vector3 GetCurrentOrientationNormal()
            { return transform.rotation * (defaultLocalPoleNormal); }

        }
    }

}