using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we handling reference pose for look algorithms to support syncing look animation with keyframe animation
    /// </summary>
    public partial class FLookAnimator
    {

        /// <summary>
        /// Detecting if animator changed animation and running transition for reference pose
        /// Should be checked every frame if using "Monitor Animator" toggle
        /// </summary>
        //private void MonitorAnimatorCalculations()
        //{
        //    if (animator && UseBoneOffsetRotation)
        //    {
        //        if (!animator.IsInTransition(0))
        //        {
        //            int currentClipHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

        //            // When animation clip changed
        //            if (currentClipHash != lastClipHash) RefreshReferencePose();

        //            lastClipHash = currentClipHash;
        //        }
        //    }
        //}

        private int lastClipHash;




        /// <summary>
        /// Reference pose memory for look animation accordance with keyframe animation
        /// Called once at start or every animation change with "Monitor Animator"
        /// </summary>
        private void RefreshReferencePose()
        {
            for (int i = 0; i < LookBones.Count; i++)
                LookBones[i].RefreshStaticRotation(!MonitorAnimator);

            if (MonitorAnimator)
            {
                StopCoroutine(CRefreshReferencePose());
                StartCoroutine(CRefreshReferencePose());
            }

            refreshReferencePose = false;
        }

        private bool refreshReferencePose = false;
        private float monitorTransitionTime = 0.8f;




        /// <summary>
        /// Refreshing reference pose for animation
        /// </summary>
        private IEnumerator CRefreshReferencePose()
        {
            // Wait for animation transition in animator and then remembering pose
            yield return null;
            yield return new WaitForSecondsRealtime(0.05f);

            // Preparing variables to proceed transition
            if (_monitorTransitionStart == null) _monitorTransitionStart = new List<Quaternion>();


            if (_monitorTransitionStart.Count != LookBones.Count)
                for (int i = 0; i < LookBones.Count; i++)
                    _monitorTransitionStart.Add(LookBones[i].animatedStaticRotation);

            for (int i = 0; i < LookBones.Count; i++)
                LookBones[i].RefreshStaticRotation(false);

            // Doing transition
            float elapsed = 0f;
            while (elapsed < monitorTransitionTime)
            {
                elapsed += delta;
                float progress = FEasing.EaseInOutCubic(0f, 1f, elapsed / monitorTransitionTime);

                for (int i = 0; i < LookBones.Count; i++)
                {
                    LookBones[i].animatedStaticRotation = Quaternion.Slerp(_monitorTransitionStart[i], LookBones[i].targetStaticRotation, progress);
                }

                yield return null;
            }

            // Finishing
            for (int i = 0; i < LookBones.Count; i++)
                LookBones[i].animatedStaticRotation = LookBones[i].targetStaticRotation;

            yield break;
        }

        List<Quaternion> _monitorTransitionStart;

    }
}