using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement direction targeting animation behaviour
    /// </summary>
    public partial class FLookAnimator
    {
        /// <summary> Stiff instant target look rotation without any clamp limits and other features</summary>
        private Vector3 unclampedLookAngles = Vector3.zero;
        /// <summary> Stiff instant target local rotation </summary>
        private Vector3 targetLookAngles = Vector3.zero;
        /// <summary> Local rotation angles which are changed every frame to smoothly transitio towards target </summary>
        private Vector3 animatedLookAngles = Vector3.zero;
        /// <summary> Animated local look rotation with 'LookAnimatorAmount' weights blending </summary>
        private Vector3 finalLookAngles = Vector3.zero;
        /// <summary> Variable needed for base rotation compensation </summary>
        private Quaternion lastBaseRotation;

        // Helper variables for internal calculations
        private Vector3 _preLookAboveLookAngles = Vector3.zero;
        private Vector3 _velo_animatedLookAngles = Vector3.zero;
        private float _rememberSideLookHorizontalAngle = 0f;


        /// <summary>
        /// Calculating angles for look animation with limitations and behaviours settings
        /// </summary>
        public Vector2 LimitAnglesCalculations(Vector2 angles)
        {
            // If our target is out of max distance let's look at default direction
            if (LookState == EFHeadLookState.OutOfMaxDistance)
            {
                angles = Vector2.MoveTowards(angles, Vector2.zero, 2.75f + RotationSpeed);
                return angles;
            }

            unclampedLookAngles = angles;


            // Exceeding max look rotation -----------------------------------------------------

            if (LookState != EFHeadLookState.OutOfMaxRotation) // If we not yet exceeded rotation range
            {
                if (Mathf.Abs(angles.y) > StopLookingAbove)
                {
                    LookState = EFHeadLookState.OutOfMaxRotation;
                    return angles;
                }
            }
            else // If state of exceed is marked we checking if we can remove this state
            {
                if (Mathf.Abs(angles.y) <= StopLookingAbove * StopLookingAboveFactor)
                {
                    LookState = EFHeadLookState.Null;
                }
                else
                {
                    angles = Vector3.MoveTowards(angles, Vector2.zero, 2.75f + RotationSpeed);
                    return angles;
                }
            }

            if (LookState == EFHeadLookState.Null) LookState = EFHeadLookState.Following;


            #region Clamping look angles when looking at target Feature


            if (LookState == EFHeadLookState.Following || LookState == EFHeadLookState.ClampedAngle)
            {

                // Clamping Horizontal look axis - looking left and right

                if (angles.y < XRotationLimits.x) // Looking to the left clamp angle
                {
                    angles.y = GetClampedAngle(unclampedLookAngles.y, XRotationLimits.x, XElasticRange, -1f);
                    if (angles.y < unclampedLookAngles.y) angles.y = unclampedLookAngles.y;
                    LookState = EFHeadLookState.ClampedAngle;
                }
                else if (angles.y > XRotationLimits.y) // Looking to the right clamp angle
                {
                    angles.y = GetClampedAngle(unclampedLookAngles.y, XRotationLimits.y, XElasticRange);
                    if (angles.y > unclampedLookAngles.y) angles.y = unclampedLookAngles.y;

                    LookState = EFHeadLookState.ClampedAngle;
                }
                else
                {
                    LookState = EFHeadLookState.Following;
                }


                // Clamping Vertical look axis - looking up or down

                if (angles.x < YRotationLimits.x) // Looking to the up clamp angle
                {
                    angles.x = GetClampedAngle(angles.x, YRotationLimits.x, YElasticRange, -1f);
                    if (angles.x < unclampedLookAngles.x) angles.x = unclampedLookAngles.x;

                    LookState = EFHeadLookState.ClampedAngle;
                }
                else if (angles.x > YRotationLimits.y) // Looking to the down clamp angle
                {
                    angles.x = GetClampedAngle(angles.x, YRotationLimits.y, YElasticRange);
                    if (angles.x > unclampedLookAngles.x) angles.x = unclampedLookAngles.x;

                    LookState = EFHeadLookState.ClampedAngle;
                }
                else
                {
                    if (LookState != EFHeadLookState.ClampedAngle) LookState = EFHeadLookState.Following;
                }
            }

            #endregion


            #region Elastic Start Look Angle Feature

            // Horizontal start look elastic range
            if (StartLookElasticRangeX > 0f)
            {
                //float angle = angles.y;
                float toTarget = Mathf.Abs(angles.y) / StartLookElasticRangeX;
                angles.y = Mathf.Lerp(0f, angles.y, toTarget); //FEasing.EaseInOutCubic(0f, targetLocalLookRotation.y, toTarget);
            }

            // Vertical start look elastic range
            if (StartLookElasticRangeY > 0f)
            {
                //float angle = angles.x;
                float toTarget = Mathf.Abs(angles.x) / StartLookElasticRangeY;
                angles.x = Mathf.Lerp(0f, angles.x, toTarget); //FEasing.EaseInOutCubic(0f, targetLocalLookRotation.y, toTarget);
            }

            #endregion


            #region If character would need to turn to other side - HoldRotateToOppositeUntil Feature

            if (HoldRotateToOppositeUntil > 0f)
            {
                int side = 0;
                if (_rememberSideLookHorizontalAngle > 0f && unclampedLookAngles.y < 0f) side = 1; // From right to left
                else
                if (_rememberSideLookHorizontalAngle < 0f && unclampedLookAngles.y > 0f) side = -1; // From left to right

                // Sign difference check
                if (side != 0)
                {
                    if (side < 0)
                    {
                        if (unclampedLookAngles.y < 180 - HoldRotateToOppositeUntil)
                            _rememberSideLookHorizontalAngle = angles.y;
                        else
                            angles.y = _rememberSideLookHorizontalAngle;
                    }
                    else
                    {
                        if (-unclampedLookAngles.y < 180 - HoldRotateToOppositeUntil)
                            _rememberSideLookHorizontalAngle = angles.y;
                        else
                            angles.y = _rememberSideLookHorizontalAngle;
                    }
                }
                else // Same Sign
                {
                    _rememberSideLookHorizontalAngle = angles.y;
                }
            }

            #endregion


            #region Look when above feature


            if (LookWhenAbove > 0)
            {
                whenAboveGoBackAngles = angles;

                // Horizontal
                float angle = Mathf.Abs(Mathf.DeltaAngle(_preLookAboveLookAngles.y, angles.y));

                if (angle < animatedLookWhenAbove)
                    angles.y = _preLookAboveLookAngles.y;
                else
                {
                    angles.y = Mathf.LerpUnclamped(_preLookAboveLookAngles.y, angles.y, (angle - animatedLookWhenAbove) / angle);
                    _preLookAboveLookAngles.y = angles.y;
                }

                // Vertical
                float limit = animatedLookWhenAboveVertical > 0f ? animatedLookWhenAboveVertical : animatedLookWhenAbove;
                angle = Mathf.Abs(Mathf.DeltaAngle(_preLookAboveLookAngles.x, angles.x));

                if (angle < limit)
                    angles.x = _preLookAboveLookAngles.x;
                else
                {
                    angles.x = Mathf.LerpUnclamped(_preLookAboveLookAngles.x, angles.x, (angle - limit) / angle);
                    _preLookAboveLookAngles.x = angles.x;
                }
            }

            #endregion


            return angles;
        }


        /// <summary>
        /// Transitioning angle values toward target with smooth animation
        /// </summary>
        public Vector2 AnimateAnglesTowards(Vector2 angles)
        {
            if (!usingAxisCorrection)
            {
                Vector3 off = (BaseTransform.rotation * Quaternion.Inverse(lastBaseRotation)).eulerAngles;
                off = WrapVector(off) * BaseRotationCompensation;
                animatedLookAngles -= off;
            }

            if (!instantRotation)
            {

                switch (AnimationStyle)
                {
                    case EFAnimationStyle.SmoothDamp:

                        float rotationTime; // Calculating rotation time in responsive way for 'RotationSpeed' parameter

                        if (RotationSpeed < 0.8f)
                            rotationTime = Mathf.Lerp(0.4f, 0.18f, RotationSpeed / 0.8f);
                        else if (RotationSpeed < 1.7f)
                            rotationTime = Mathf.Lerp(0.18f, 0.1f, (RotationSpeed - 0.8f) / (1.7f - 0.8f));
                        else if (RotationSpeed < 2.15f)
                            rotationTime = Mathf.Lerp(0.1f, 0.05f, (RotationSpeed - 1.7f) / (2.15f - 1.7f));
                        else
                            rotationTime = Mathf.Lerp(0.05f, 0.02f, (RotationSpeed - 2.15f) / (2.5f - 2.15f));

                        rotationTime *= smoothingEffect;

                        float maxRotationSpeed; // Calculating max rotation speed in responsive way for 'MaxRotationSpeed' parameter

                        if (MaxRotationSpeed >= 2.5f) maxRotationSpeed = Mathf.Infinity;
                        else
                        {
                            if (MaxRotationSpeed < 0.8f)
                                maxRotationSpeed = Mathf.Lerp(100f, 430f, MaxRotationSpeed / 0.8f);
                            else if (MaxRotationSpeed < 1.7f)
                                maxRotationSpeed = Mathf.Lerp(430f, 685f, (MaxRotationSpeed - 0.8f) / (1.7f - 0.8f));
                            else 
                                maxRotationSpeed = Mathf.Lerp(685f, 1250f, (MaxRotationSpeed - 1.7f) / (2.5f - 1.7f));
                        }


                        animatedLookAngles = Vector3.SmoothDamp(animatedLookAngles, angles, ref _velo_animatedLookAngles, rotationTime, maxRotationSpeed, delta);

                        break;

                    case EFAnimationStyle.FastLerp:

                        float lerpSpeed; // Calculating lerpSpeed in responsive way for 'RotationSpeed' parameter

                        if (RotationSpeed < 0.8f)
                            lerpSpeed = Mathf.Lerp(2.85f, 4.5f, RotationSpeed / 0.8f);
                        else if (RotationSpeed < 1.7f)
                            lerpSpeed = Mathf.Lerp(4.5f,10f, (RotationSpeed - 0.8f) / (1.7f - 0.8f));
                        else if (RotationSpeed < 2.15f)
                            lerpSpeed = Mathf.Lerp(10f, 14f, (RotationSpeed - 1.7f) / (2.15f - 1.7f));
                        else
                            lerpSpeed = Mathf.Lerp(14f, 25f, (RotationSpeed - 2.15f) / (2.5f - 2.15f));

                        lerpSpeed /= smoothingEffect;

                        Vector3 target = Vector3.Lerp(animatedLookAngles, angles, lerpSpeed * delta);


                        if (MaxRotationSpeed < 2.5f)
                        {
                            float maxDeltaSpeed; // Calculating max delta speed in responsive way for 'MaxRotationSpeed' parameter

                            if (MaxRotationSpeed <1.1f)
                                maxDeltaSpeed = Mathf.Lerp(5f, 9f, MaxRotationSpeed / 1.1f);
                            else if (MaxRotationSpeed < 1.7f)
                                maxDeltaSpeed = Mathf.Lerp(9f, 20f, (MaxRotationSpeed - 1.1f) / (1.7f - 1.1f));
                            else
                                maxDeltaSpeed = Mathf.Lerp(20f, 45f, (MaxRotationSpeed - 1.7f) / (2.5f - 1.7f));

                            float diff = Vector3.Distance(target, animatedLookAngles);
                            
                            if (diff > maxDeltaSpeed) lerpSpeed /= (1f + (diff - maxDeltaSpeed) / 3f);

                            target = Vector3.Lerp(animatedLookAngles, angles, lerpSpeed * delta);
                        }

                        animatedLookAngles = target;

                        break;

                    case EFAnimationStyle.Linear:

                        animatedLookAngles = Vector3.MoveTowards(animatedLookAngles, angles, delta * (.2f + RotationSpeed) * 300f);

                        break;
                }
            }
            else // If using instant rotation we just set target calculated rotation
                animatedLookAngles = angles;


            finalLookAngles = Vector3.LerpUnclamped(Vector3.zero, animatedLookAngles, finalMotionWeight);

            return finalLookAngles;
        }

    }
}