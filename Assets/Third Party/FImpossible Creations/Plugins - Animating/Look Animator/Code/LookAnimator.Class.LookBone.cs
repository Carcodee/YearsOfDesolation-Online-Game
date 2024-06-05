using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement main look bone entities, core of Look Animator
    /// </summary>
    public partial class FLookAnimator
    {
        [System.Serializable]
        public class LookBone
        {
            /// <summary> Bone transform in world </summary>
            public Transform Transform;
            //public Transform BaseTransform;

            #region Quaternion Variables

            /// <summary> Rotations captured from one freeze frame </summary>
            public Quaternion animatedStaticRotation;
            /// <summary> StaticRotation target value used when monitoring animator </summary>
            public Quaternion targetStaticRotation;
            /// <summary> Local rotations captured from one freeze frame -> Used when using parental fixing method and not syncing with animator </summary>
            public Quaternion localStaticRotation;

            /// <summary> Smoothly changed toward bone target rotation </summary>
            public Quaternion animatedTargetRotation;
            /// <summary> Rotation to which bone will rotate towards, can be used in extension calculations </summary>
            public Quaternion targetRotation;

            /// <summary> "Additional Modules" bone rotation offset mainly for correcting pose of character </summary>
            //public Quaternion correctionOffset;
            public Vector3 correctionOffset;
            public Quaternion correctionOffsetQ { get { return Quaternion.Euler(correctionOffset); } }

            /// <summary> Rotation value of target bone + offset or just target rotation if not syncing with animator </summary>
            public Quaternion finalRotation;

            /// <summary> Variable to detect if some bones are not keyframed in animation to avoid spinning glitch </summary>
            public Quaternion lastKeyframeRotation;
            public Quaternion lastFinalLocalRotation;

            #endregion


            #region Additional helper rotation variables

            public Vector3 forward;
            public Vector3 right;
            public Vector3 up;

            #endregion


            #region Position Variables (Bird Mode)

            /// <summary> Initial local position of bone for animating bone position </summary>
            public Vector3 initLocalPos = Vector3.zero;
            public Quaternion initLocalRot = Quaternion.identity;
            /// <summary> Variable for laggy movement of bird mode </summary>
            public Vector3 targetDelayPosition;
            /// <summary> Variable for laggy movement of bird mode </summary>
            public Vector3 animatedDelayPosition;

            #endregion


            /// <summary> Base bone rotation weight factor </summary>
            public float lookWeight = 1f;
            /// <summary> Look weight value for big angle look </summary>
            public float lookWeightB = 1f;
            /// <summary> Playmode active bone weight values </summary>
            public float motionWeight = 1f;


            public LookBone(Transform t)
            {
                Transform = t;
                correctionOffset = Vector3.zero;

                if (t != null)
                {
                    initLocalPos = t.localPosition;
                    initLocalRot = t.localRotation;
                }
            }

            public void RefreshBoneDirections(Transform baseTransform)
            {
                if (Transform == null) return;
                forward = Quaternion.FromToRotation(Transform.InverseTransformDirection(baseTransform.forward), Vector3.forward) * Vector3.forward;
                up = Quaternion.FromToRotation(Transform.InverseTransformDirection(baseTransform.up), Vector3.up) * Vector3.up;
                right = Quaternion.FromToRotation(Transform.InverseTransformDirection(baseTransform.right), Vector3.right) * Vector3.right;
            }

            public void RefreshStaticRotation(bool hard = true)
            {
                targetStaticRotation = Transform.rotation;
                if (initLocalPos == Vector3.zero) initLocalPos = Transform.localPosition;

                if (hard)
                {
                    animatedStaticRotation = targetStaticRotation;
                }

                localStaticRotation = Transform.localRotation;
            }


            /// <summary> Animated bone transition </summary>
            internal void CalculateMotion(Quaternion targetLook, float overallWeightMultiplier, float delta, float mainWeight)
            {
                // Getting weighted look rotation for bone
                targetRotation = GetTargetRot( targetLook, motionWeight * overallWeightMultiplier);
                //targetRotation = Quaternion.LerpUnclamped(Quaternion.identity, targetLook, motionWeight * overallWeightMultiplier);
                if (delta < 1f) animatedTargetRotation = Quaternion.LerpUnclamped(animatedTargetRotation, targetRotation, delta); else animatedTargetRotation = targetRotation;

                finalRotation = Quaternion.LerpUnclamped(Transform.rotation, animatedTargetRotation * Transform.rotation, mainWeight); // Getting target rotation
            }

            internal Quaternion GetTargetRot(Quaternion targetLook, float weight)
            {
                return Quaternion.LerpUnclamped(Quaternion.identity, targetLook, weight);
            }

        }


    }

}