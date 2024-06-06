using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Limb : FIK_ProcessorBase
    {
        // Foot/End Bone rotation helper with root reference
        public Quaternion EndBoneMapping { get; protected set; }


        /// <summary> Assigning helpful reference to main root transform of body to help IK rotations </summary>
        public virtual void SetRootReference(Transform mainParentTransform)
        {
            Root = mainParentTransform;
            EndBoneMapping = Quaternion.FromToRotation(EndIKBone.right, Vector3.right);
            EndBoneMapping *= Quaternion.FromToRotation(EndIKBone.up, Vector3.up);
        }



        /// <summary> Reference scale for computations - active length from start bone to middle knee </summary>
        public float ScaleReference { get; protected set; }

        public void RefreshLength()
        {
            ScaleReference = (StartIKBone.transform.position - MiddleIKBone.transform.position).magnitude;
        }

        public void RefreshScaleReference()
        {
            ScaleReference = (StartIKBone.transform.position - MiddleIKBone.transform.position).magnitude;
        }


        /// <summary> Returning >= 1f when max range for IK point is reached </summary>
        public float GetStretchValue(Vector3 targetPos)
        {
            float fullLength = Mathf.Epsilon;
            fullLength += (StartIKBone.transform.position - MiddleIKBone.transform.position).magnitude;
            fullLength += (MiddleIKBone.transform.position - EndIKBone.transform.position).magnitude;

            float toGoal = (StartIKBone.transform.position - targetPos).magnitude;

            return toGoal / fullLength;
        }


    }
}
