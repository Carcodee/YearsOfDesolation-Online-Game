using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement feature of nodding transitions when switching to other look targets
    /// </summary>
    public partial class FLookAnimator
    {
        [Range(-1f, 1f)]
        [Tooltip("When switching targets character will make small nod to make it look more natural, set higher value for toony effect")]
        public float NoddingTransitions = 0f;
        public Vector3 NodAxis = Vector3.right;// { get; private set; }
        [Range(-1f, 1f)]
        [Tooltip("Set zero to use only leading bone, set -1 to 1 to spread this motion over backbones")]
        public float BackBonesNod = .15f;

        private float nodProgress = 0f;
        private float nodValue = 0f;
        private float nodPower = 0f;
        private float nodDuration = 1f;


        /// <summary>
        /// Triggering nodding when changing target
        /// </summary>
        private void NoddingChangeTargetCalculations(float angleAmount)
        {
            if (nodProgress < nodDuration / 10f || nodProgress > nodDuration * 0.85f)
                if (NoddingTransitions != 0f)
                {
                    nodProgress = 0f;

                    nodDuration = Mathf.Lerp(1.0f, 0.45f, RotationSpeed / 2.5f);
                    if ( ChangeTargetSmoothing > 0f) nodDuration *= Mathf.Lerp(1f, 1.55f, ChangeTargetSmoothing);
                    nodDuration *= Mathf.Lerp(0.8f, 1.4f, Mathf.InverseLerp(10f, 140f, angleAmount));

                    nodPower = Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(8f, 55f, angleAmount));
                }
        }


        /// <summary>
        /// Simple cosinuosidal animation during transitioning to new target
        /// </summary>
        private void NoddingCalculations()
        {
            if (nodProgress < nodDuration)
            {
                if (nodProgress < nodDuration) nodProgress += delta; else nodProgress = nodDuration;

                float progress = nodProgress / nodDuration;
                progress = FEasing.EaseOutCubic(0f, 1f, progress);

                if (progress >= 1f) nodValue = 0f;
                else
                    nodValue = Mathf.Sin(progress * (Mathf.PI));
            }
        }

    }
}