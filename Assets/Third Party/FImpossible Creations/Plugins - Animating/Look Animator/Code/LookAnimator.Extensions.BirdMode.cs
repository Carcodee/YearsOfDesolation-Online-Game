using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we implement feature of bird-like sudden head moves
    /// </summary>
    public partial class FLookAnimator
    {

        [Tooltip("Enabling laggy movement for head and delaying position")]
        public bool BirdMode = false;
        private bool birdModeInitialized = false;

        [FPD_Suffix(0f, 1f)]
        [Tooltip("Bird mode laggy movement for neck amount, lowering this value will cause crossfade motion of laggy movement and basic follow rotation")]
        public float LagRotation = 0.85f;
        [Tooltip("How often should be acquired new target position for laggy movement, time to trigger it will be slightly randomized")]
        [FPD_Suffix(0.1f, 1f, FPD_SuffixAttribute.SuffixMode.FromMinToMax, "sec")]
        public float LagEvery = 0.285f;

        [FPD_Percentage(0f, 1f)]
        [Tooltip("Bird mode keeping previous position until distance is reached")]
        public float DelayPosition = 0f;
        [Tooltip("How far distance to go back should have head to move (remind movement of pigeons to yourself)")]
        public float DelayMaxDistance = 0.25111f;
        [Tooltip("How quick head and neck should go back to right position after reaching distance")]
        [Range(0f, 1f)]
        public float DelayGoSpeed = .6f;

        public Vector3 BirdTargetPosition = Vector3.forward;
        private Vector3 birdTargetPositionMemory = Vector3.forward;
        private float lagTimer;


        private void InitBirdMode()
        {
            if (birdModeInitialized) return;

            lagTimer = 0f;
            birdTargetPositionMemory = GetLookAtPosition();
            BirdTargetPosition = birdTargetPositionMemory;

            birdModeInitialized = true;
        }


        /// <summary>
        /// Handling eye lag simulation
        /// </summary>
        private void CalculateBirdMode()
        {
            lagTimer -= delta;

            if (lagTimer < 0f) birdTargetPositionMemory = smoothLookPosition;

            if (LagRotation >= 1f) BirdTargetPosition = birdTargetPositionMemory;
            else
                BirdTargetPosition = Vector3.Lerp( smoothLookPosition, birdTargetPositionMemory, LagRotation);


            if (lagTimer < 0f) lagTimer = Random.Range(LagEvery * 0.85f, LagEvery * 1.15f);


            #region Head neck positions holding animation

            if (DelayPosition > 0f)
            {
                // Resetting default positions in animation frame
                for (int i = 0; i < LookBones.Count; i++) LookBones[i].Transform.localPosition = LookBones[i].initLocalPos;

                // Detecting movement offset to trigger new neck movement position
                float xzDist = Vector3.Distance(Vector3.Scale(LookBones[0].targetDelayPosition, new Vector3(1, 0, 1)), Vector3.Scale(LeadBone.position, new Vector3(1, 0, 1)));
                float yDist = Mathf.Abs(LookBones[0].targetDelayPosition.y - LeadBone.position.y);

                if (xzDist > DelayMaxDistance || yDist > DelayMaxDistance / 1.65f)
                {
                    for (int i = LookBones.Count-1; i >= 0; i--) LookBones[i].targetDelayPosition = LookBones[i].Transform.position;
                }

                for (int i = LookBones.Count - 1; i >= 0; i--)
                {
                    LookBones[i].animatedDelayPosition = Vector3.Lerp(LookBones[i].animatedDelayPosition, LookBones[i].targetDelayPosition, delta * Mathf.Lerp(5f, 30f, DelayGoSpeed));
                    LookBones[i].Transform.position = Vector3.Lerp(LookBones[i].Transform.position, LookBones[i].animatedDelayPosition, LookBones[i].lookWeight * DelayPosition * finalMotionWeight);
                }
            }

            #endregion

        }

    }
}