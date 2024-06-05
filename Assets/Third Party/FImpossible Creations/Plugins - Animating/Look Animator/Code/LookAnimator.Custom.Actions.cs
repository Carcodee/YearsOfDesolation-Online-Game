using UnityEngine;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator
    {
        /// <summary>
        /// Tell controller to stop or start looking without changing target follow object.
        /// </summary>
        /// <param name="enableLooking"> if look == null looking will be toggled </param>
        /// <param name="transitionTime"> Transition time in seconds </param>
        public void SwitchLooking(bool? enableLooking = null, float transitionTime = 0.2f, System.Action callback = null)
        {
            bool enableAnimation = true;

            if (enableLooking == null)
            {
                if (LookAnimatorAmount > 0.5f) enableAnimation = false;
            }
            else if (enableLooking == false) enableAnimation = false;

            StopAllCoroutines();
            StartCoroutine(SwitchLookingTransition(transitionTime, enableAnimation, callback));
        }

        /// <summary>
        /// Short version of switch looking for event handles to draw
        /// </summary>
        public void SwitchLooking(bool enable = true)
        {
            SwitchLooking(enable, 0.5f /* How long transition should take*/);
        }

        /// <summary>
        /// Setting new target to follow by head bones
        /// </summary>
        public void SetLookTarget(Transform transform)
        {
            ObjectToFollow = transform;
            MomentLookTransform = null;
        }

        /// <summary>
        /// Switching look mode to world position follow and assigning position to follow
        /// </summary>
        public void SetLookPosition(Vector3 targetPosition)
        {
            FollowMode = EFFollowMode.FollowJustPosition;
            FollowOffset = targetPosition;
        }


        /// <summary>
        /// Accessing degrees values for character look angles.
        /// If target to look is just on the right of character then return something around (0, 90)
        /// If target to look is just behind character then return something around (0, 180)
        /// </summary>
        public Vector2 GetUnclampedLookAngles()
        {
            return unclampedLookAngles;
        }

        /// <summary>
        /// Accessing current animated degrees values for character look angles clamped on look range settings.
        /// </summary>
        public Vector2 GetLookAngles()
        {
            return animatedLookAngles;
        }

        /// <summary>
        /// Accessing target degrees values (skipping rotation animation) for character look angles clamped on look range settings.
        /// </summary>
        public Vector2 GetTargetLookAngles()
        {
            return targetLookAngles;
        }

        /// <summary>
        /// Returning current state of look animator
        /// </summary>
        public EFHeadLookState GetCurrentLookState()
        {
            return LookState;
        }


        /// <summary>
        /// Computing look angles needed to look at target world position from this character
        /// </summary>
        public Vector2 ComputeAnglesTowards(Vector3 worldPosition)
        {
            // Direction towards target in different spaces
            Vector3 worldDirectionAndTargetAngles = (worldPosition - GetLookStartMeasurePosition()).normalized;

            // Supporting different axis orientation using matrix
            if (usingAxisCorrection)
            {
                worldDirectionAndTargetAngles = axisCorrectionMatrix.inverse.MultiplyVector(worldDirectionAndTargetAngles).normalized;
                worldDirectionAndTargetAngles = WrapVector(Quaternion.LookRotation(worldDirectionAndTargetAngles, axisCorrectionMatrix.MultiplyVector(ModelUpAxis).normalized).eulerAngles);
            }
            else // Easier calculations when using standard z - forward - y up
            {
                worldDirectionAndTargetAngles = BaseTransform.InverseTransformDirection(worldDirectionAndTargetAngles);
                worldDirectionAndTargetAngles = WrapVector(Quaternion.LookRotation(worldDirectionAndTargetAngles, BaseTransform.TransformDirection(ModelUpAxis)).eulerAngles);
            }

            return worldDirectionAndTargetAngles;
        }
    }
}