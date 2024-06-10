using UnityEngine;

#if UNITY_EDITOR
#endif

namespace IKPn
{
    [System.Serializable]
    public class UpperBodySetup
    {
        public Transform chest;
        [HideInInspector] public Quaternion chestRotationOffset;

        public Transform spine;
        [HideInInspector] public Quaternion spineRotationOffset;

        [HideInInspector] public Quaternion hipsChestOffset;
        [HideInInspector] public Quaternion hipsSpineOffset;

        public Transform hips;
        [HideInInspector] public Quaternion hipsRotationOffset;

        public Transform leftHand;
        [HideInInspector] public Quaternion leftHandRotationOffset;

        public Transform leftElbow;
        [HideInInspector] public Quaternion leftElbowRotationOffset;

        public Transform leftShoulder;
        [HideInInspector] public Quaternion leftShoulderRotationOffset;

        public Transform rightHand;
        [HideInInspector] public Quaternion rightHandRotationOffset;

        public Transform rightElbow;
        [HideInInspector] public Quaternion rightElbowRotationOffset;

        public Transform rightShoulder;
        [HideInInspector] public Quaternion rightShoulderRotationOffset;

        [HideInInspector] public float leftElbowLength;

        [HideInInspector] public float leftHandLength;

        [HideInInspector] public float rightElbowLength;

        [HideInInspector] public float rightHandLength;
    }
}
