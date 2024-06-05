using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement core start look / look at target measure position methods
    /// </summary>
    public partial class FLookAnimator
    {

        /// <summary>
        /// Getting position for measuring distance from targeted positions
        /// </summary>
        public Vector3 GetDistanceMeasurePosition()
        {
            return BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint);
        }


        /// <summary>
        /// Look start reference position to measure direction from
        /// </summary>
        public Vector3 GetLookStartMeasurePosition()
        {
            _LOG_NoRefs();

            if (AnchorStartLookPoint)
            {
                // Supporting different axis orientation using matrix
                if (usingAxisCorrection)
                {
                    if (!Application.isPlaying) UpdateCorrectionMatrix();

                    if (leadBoneInitLocalOffset == Vector3.zero) return LeadBone.position + axisCorrectionMatrix.MultiplyVector(StartLookPointOffset);
                    return axisCorrectionMatrix.MultiplyPoint(leadBoneInitLocalOffset) + axisCorrectionMatrix.MultiplyVector(StartLookPointOffset);

                }
                else
                {
                    if (leadBoneInitLocalOffset == Vector3.zero) return LeadBone.position + BaseTransform.TransformVector(StartLookPointOffset);
                    return BaseTransform.TransformPoint(leadBoneInitLocalOffset) + BaseTransform.TransformVector(StartLookPointOffset);
                }
            }
            else
            {
                if (!Application.isPlaying) LookBones[0].finalRotation = LeadBone.transform.rotation;
                return LeadBone.position + LookBones[0].finalRotation * StartLookPointOffset;
            }
        }


        private Vector3 leadBoneInitLocalOffset = Vector3.zero;
        /// <summary>
        /// Anchoring reference position for look start point basing on current frame's pose of LeadBone
        /// </summary>
        public void RefreshLookStartPositionAnchor()
        {
            if (!usingAxisCorrection)
                leadBoneInitLocalOffset = BaseTransform.InverseTransformPoint(LeadBone.position);
            else
                leadBoneInitLocalOffset = axisCorrectionMatrix.inverse.MultiplyPoint(LeadBone.position);

            RefreshStartLookPoint = false;
        }


        /// <summary>
        /// Getting distance value from distance measure point to target position
        /// </summary>
        private float GetDistanceMeasure(Vector3 targetPosition)
        {
            if (Distance2D)
            {
                Vector3 p = GetDistanceMeasurePosition();
                Vector2 p2 = new Vector2(p.x, p.z);
                return Vector2.Distance(p2, new Vector2(targetPosition.x, targetPosition.z));
            }
            else
            {
                return Vector3.Distance(GetDistanceMeasurePosition(), targetPosition);
            }
        }


    }
}