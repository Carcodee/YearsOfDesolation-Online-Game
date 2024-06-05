using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement class for compensation bones - so if character is looking hardly up or down, we don't want arms to rotate so much with pelvis
    /// </summary>
    public partial class FLookAnimator
    {
        [System.Serializable]
        public class CompensationBone
        {
            public Transform Transform;
            private Vector3 compensatedPosition;
            private Quaternion compensatedRotation;

            private Quaternion lastFinalLocalRotation;
            private Quaternion lastKeyframeLocalRotation;
            private Vector3 lastFinalLocalPosition;
            private Vector3 lastKeyframeLocalPosition;
            //private Vector3 initLocalPos;


            public CompensationBone(Transform t)
            {
                Transform = t;
                if (t)
                {
                    //initLocalPos = t.localPosition;
                    lastKeyframeLocalPosition = t.localPosition;
                    lastKeyframeLocalRotation = t.localRotation;
                }
            }


            public void RefreshCompensationFrame()
            {
                compensatedPosition = Transform.position;
                compensatedRotation = Transform.rotation;
            }



            public void CheckForZeroKeyframes()
            {
                if (FEngineering.QIsSame(lastFinalLocalRotation, Transform.localRotation))
                {
                    Transform.localRotation = lastKeyframeLocalRotation;
                    compensatedRotation = Transform.rotation;
                }
                else
                {
                    lastKeyframeLocalRotation = Transform.localRotation;
                }

                if (FEngineering.VIsSame(lastFinalLocalPosition, Transform.localPosition))
                {
                    Transform.localPosition = lastKeyframeLocalPosition;
                    compensatedPosition = Transform.position;
                }
                else
                {
                    lastKeyframeLocalPosition = Transform.localPosition;
                }
            }


            public void SetRotationCompensation(float weight)
            {
                if (weight > 0f)
                {
                    if (weight >= 1f) Transform.rotation = compensatedRotation;
                    else
                        Transform.rotation = Quaternion.LerpUnclamped(Transform.rotation, compensatedRotation, weight);

                    lastFinalLocalRotation = Transform.localRotation;
                }
            }


            public void SetPositionCompensation(float weight)
            {
                if (weight > 0f)
                {
                    if (weight >= 1f) Transform.position = compensatedPosition;
                    else
                        Transform.position = Vector3.LerpUnclamped(Transform.position, compensatedPosition, weight);

                    lastFinalLocalPosition = Transform.localPosition;
                }
            }
        }
    }
}