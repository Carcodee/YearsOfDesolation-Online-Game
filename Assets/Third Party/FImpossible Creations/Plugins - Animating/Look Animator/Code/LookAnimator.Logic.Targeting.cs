using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we handle base variables and methods to controll look at target coordinates
    /// </summary>
    public partial class FLookAnimator
    {
        /// <summary> Current target transform to follow </summary>
        private Transform activeLookTarget;

        /// <summary> Current target position to follow </summary>
        private Vector3 activeLookPosition;

        /// <summary> Remembering target transform since last frame to detect if something changed </summary>
        private Transform preActiveLookTarget;

        /// <summary> If look state is out of range for looking at object - when animation is going back to default pose </summary>
        private bool isLooking = false;

        [Tooltip("If moment transform should be destroyed when max distance range is exceed")]
        public bool DestroyMomentTargetOnMaxDistance = true;



        /// <summary>
        /// Updating variable smoothLookPosition which is position at which object is looking
        /// </summary>
        private void LookPositionUpdate()
        {
            if (LookAtPositionSmoother > 0f) // Smoothed look position
                smoothLookPosition = Vector3.SmoothDamp(smoothLookPosition, activeLookPosition, ref _velo_smoothLookPosition, LookAtPositionSmoother / 2f, Mathf.Infinity, delta);
            else
                smoothLookPosition = activeLookPosition;
        }


        /// <summary>
        /// Updating state of current look target
        /// Called every frame
        /// </summary>
        private void TargetingUpdate()
        {
            activeLookTarget = GetLookAtTransform();

            activeLookPosition = GetLookAtPosition();

            if (preActiveLookTarget != activeLookTarget) OnTargetChanged();
        }



        /// <summary>
        /// Getting position to be followed by Look animator, if there is no target to follow position in front of lead bone will be returned
        /// </summary>
        public Vector3 GetLookAtPosition()
        {
            _LOG_NoRefs();

            if (FollowMode == EFFollowMode.FollowJustPosition)
            {
                return FollowOffset;
            }
            else
            {
                Transform lookT = activeLookTarget;
                if (lookT == null) lookT = GetLookAtTransform();

                // If there is no target to follow we move focus point to front of lead bone
                if (!lookT)
                    return LeadBone.position + BaseTransform.TransformVector(ModelForwardAxis) * Vector3.Distance(LeadBone.position, BaseTransform.position);
                else
                {
                    if (FollowMode == EFFollowMode.ToFollowSpaceOffset)
                        return lookT.position + lookT.TransformVector(FollowOffset);
                    else if (FollowMode == EFFollowMode.WorldOffset)
                        return lookT.position + FollowOffset;
                    else if (FollowMode == EFFollowMode.LocalOffset)
                        return lookT.position + BaseTransform.TransformVector(FollowOffset);
                    else
                        return lookT.position;
                }
            }
        }



        /// <summary>
        /// Getting transform which is followed with look
        /// Including moment look targets which are prioritized over 'ObjectToFollow'
        /// </summary>
        public Transform GetLookAtTransform()
        {
            if (MomentLookTransform)
            {
                if (!wasMomentLookTransform)
                {
                    OnTargetChanged();
                    wasMomentLookTransform = true;
                }

                return MomentLookTransform;
            }

            if (!MomentLookTransform)
            {
                if (wasMomentLookTransform)
                {
                    OnTargetChanged();
                    wasMomentLookTransform = false;
                }

                if (ObjectToFollow) return ObjectToFollow;
            }

            return null;
        }



        /// <summary>
        /// Getting position in front of head bone
        /// </summary>
        public Vector3 GetForwardPosition()
        {
            return LeadBone.position + BaseTransform.TransformDirection(ModelForwardAxis);
        }



        /// <summary>
        /// Executed every time when target is changed via code or via inspector or by loosing target object reference
        /// </summary>
        protected void TargetChangedMeasures()
        {
            Vector3 lookDir = GetCurrentHeadForwardDirection();
            Vector3 headDir = preLookDir.normalized;

            Vector3 lookRotation = Quaternion.LookRotation(lookDir).eulerAngles;
            Vector3 headRotation = headDir == Vector3.zero ? Vector3.zero : Quaternion.LookRotation(headDir).eulerAngles;
            Vector3 characterRotation = Quaternion.LookRotation(transform.TransformVector(ModelForwardAxis)).eulerAngles;

            // Calculating target look rotation including clamping parameters and others
            Vector2 characterDelta = new Vector3(Mathf.DeltaAngle(lookRotation.x, characterRotation.x), Mathf.DeltaAngle(lookRotation.y, characterRotation.y));

            float maxRotOff = StopLookingAbove;
            if (Mathf.Abs(XRotationLimits.x) > StopLookingAbove) maxRotOff = Mathf.Abs(XRotationLimits.x);

            if (Mathf.Abs(characterDelta.y) > maxRotOff) lookRotation = characterRotation;
            else
            {
                if (characterDelta.y < XRotationLimits.x) lookRotation.y = characterRotation.y + XRotationLimits.y;
                if (characterDelta.y > XRotationLimits.y) lookRotation.y = characterRotation.y + XRotationLimits.x;

                if (characterDelta.x < YRotationLimits.x) lookRotation.x = characterRotation.x + XRotationLimits.x;
                if (characterDelta.x > YRotationLimits.y) lookRotation.x = characterRotation.x + XRotationLimits.y;
            }

            // Vector with degrees differences to all axes
            Vector2 deltaVector = new Vector3(Mathf.DeltaAngle(lookRotation.x, headRotation.x), Mathf.DeltaAngle(lookRotation.y, headRotation.y));

            // Gathering helpful values
            float total = Mathf.Abs(deltaVector.x) + Mathf.Abs(deltaVector.y);

            if (ChangeTargetSmoothing > 0f)
                if (total > 20f)
                    SetRotationSmoothing(Mathf.Lerp(0.15f + ChangeTargetSmoothing * 0.25f, 0.4f + ChangeTargetSmoothing * 0.2f, Mathf.InverseLerp(20f, 180f, total)), Mathf.Lerp(.7f, 3f, ChangeTargetSmoothing));

            NoddingChangeTargetCalculations(total);
        }



        /// <summary>
        /// Handling max distance feature
        /// </summary>
        private void MaxDistanceCalculations()
        {
            // If we are using distance limitation
            if (MaximumDistance > 0f)
            {
                if (isLooking) // If look motion is not out of look range etc.
                {
                    float distance = GetDistanceMeasure(activeLookPosition);

                    if (distance > MaximumDistance + MaximumDistance * MaxOutDistanceFactor)
                    {
                        LookState = EFHeadLookState.OutOfMaxDistance;
                        OnRangeStateChanged();
                        if (DestroyMomentTargetOnMaxDistance) ForceDestroyMomentTarget();
                    }
                }
                else // When look animator is not looking at target
                {
                    // Entering back distance range
                    if (LookState == EFHeadLookState.OutOfMaxDistance)
                    {
                        float distance = GetDistanceMeasure(activeLookPosition);

                        //if (MaxDistDetectionFactor >= 1f)
                        //{
                        //    if (distance <= MaximumDistance)
                        //    {
                        //        LookState = EFHeadLookState.Null;
                        //        OnRangeStateChanged();
                        //    }
                        //}
                        //else 
                        if (distance <= MaximumDistance)
                        {
                            LookState = EFHeadLookState.Null;
                            OnRangeStateChanged();
                        }
                    }
                }
            }
            else // If we don't use max of distance feature and look state is setted to outOfMaxDistance state
            {
                if (LookState == EFHeadLookState.OutOfMaxDistance) LookState = EFHeadLookState.Null;
            }
        }



        /// <summary>
        /// Called every time when follow target to look at is changing
        /// Including changing look target to moment target and looking back on 'ObjectToFollow'
        /// </summary>
        protected virtual void OnTargetChanged()
        {
            //Debug.Log("OnTargetChanged");
            TargetChangedMeasures();
        }


        /// <summary>
        /// Called every time when target object go out of sight range or distance 
        /// range and animator is trying go back to default pose
        /// </summary>
        protected virtual void OnRangeStateChanged()
        {
            //Debug.Log("OnRangeStateChanged");
            TargetChangedMeasures();
        }
    }
}