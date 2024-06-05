using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement feature of smooth motion for follow target point
    /// </summary>
    public partial class FLookAnimator
    {
        private float smoothingTimer = 0f;
        private float smoothingPower = 1f;
        private float smoothingTime = 1f;
        private float smoothingEffect = 1f;

        public void SetRotationSmoothing(float smoothingDuration = 0.5f, float smoothingPower = 2f)
        {
            if (smoothingDuration <= 0f) return;

            smoothingTimer = smoothingDuration;
            smoothingTime = smoothingDuration;
            this.smoothingPower = smoothingPower;
        }

        private void UpdateSmoothing()
        {
            if (smoothingTimer > 0f)
            {
                smoothingTimer -= delta;
                smoothingEffect = 1f + (smoothingTimer / smoothingTime) * smoothingPower;
            }
            else
            {
                smoothingEffect = 1f;
            }
        }
    }
}