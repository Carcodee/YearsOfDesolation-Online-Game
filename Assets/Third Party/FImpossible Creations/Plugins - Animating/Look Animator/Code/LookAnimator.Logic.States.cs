using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we handling controll over look states
    /// </summary>
    public partial class FLookAnimator
    {
        private EFHeadLookState previousState = EFHeadLookState.Null;
        private bool _stopLooking = false;

        /// <summary>
        /// Changing lerp value for smoothing animations - this lerp value is used in CalculateLookAnimation() method
        /// </summary>
        private void UpdateLookAnimatorAmountWeight()
        {
            if (!_stopLooking)
            {
                if (LookState == EFHeadLookState.OutOfMaxDistance || LookState == EFHeadLookState.OutOfMaxRotation || LookState == EFHeadLookState.Null)
                    _stopLooking = true;
            }

            float turbo = BirdMode ? (RotationSpeed) : 1f;

            // Smoothly changing weight of look bone animation to zero
            if (_stopLooking)
            {
                animatedMotionWeight = Mathf.SmoothDamp(animatedMotionWeight, 0f, ref _velo_animatedMotionWeight, Mathf.Lerp(.5f, 0.25f, RotationSpeed / 2.5f), Mathf.Infinity, delta * turbo);
            }
            else // Transitioning to look animation
            {
                if (previousState == EFHeadLookState.OutOfMaxRotation) OnRangeStateChanged();

                animatedMotionWeight = Mathf.SmoothDamp(animatedMotionWeight, 1f, ref _velo_animatedMotionWeight, Mathf.Lerp(.3f, 0.125f, RotationSpeed / 2.5f), Mathf.Infinity, delta * turbo);
            }

            finalMotionWeight = animatedMotionWeight * LookAnimatorAmount;
            if (finalMotionWeight > 0.999f) finalMotionWeight = 1f;
        }


        /// <summary>
        /// Finishing update calculations, remembering state of variables in update frame
        /// </summary>
        private void EndUpdate()
        {
            preActiveLookTarget = activeLookTarget;
            //preActiveLookPosition = activeLookPosition;
            preWeightFaloff = FaloffValue;
            lastBaseRotation = BaseTransform.rotation;
            preLookDir = GetCurrentHeadForwardDirection();
        }
    }
}