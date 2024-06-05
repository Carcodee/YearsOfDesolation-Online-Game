using UnityEngine;

namespace FIMSpace.FLook
{
    public class SM_SwitchLookAnimator : StateMachineBehaviour
    {
        [Tooltip("Time of animation")]
        [Range(0f, 1f)]
        public float EnableBackAfter = 0.9f;
        public float TransitionDuration = 0.3f;

        bool enableBackTriggered = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            SwitchLook(animator, false);
            enableBackTriggered = false;
        }


        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (animator.IsInTransition(layerIndex)) return;

            if (stateInfo.normalizedTime > EnableBackAfter)
            {
                if (!enableBackTriggered)
                {
                    SwitchLook(animator, true);
                    enableBackTriggered = true;
                }
            }
        }


        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (!enableBackTriggered)
            {
                SwitchLook(animator, true);
                enableBackTriggered = true;
            }
        }

        void SwitchLook(Animator animator, bool enable)
        {
            FLookAnimator look = animator.GetComponentInChildren<FLookAnimator>();
            look.SwitchLooking(enable, TransitionDuration);
        }

    }
}