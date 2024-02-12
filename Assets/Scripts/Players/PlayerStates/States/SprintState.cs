using UnityEngine;

namespace Players.PlayerStates.States
{
    public class SprintState : PlayerStateBase
    {
        public SprintState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;
        }
        float lerpTimeToSprint= 0;
        public override void StateEnter()
        {
            base.StateEnter();
            playerRef.sprintFactor = 1.5f;
        }

        public override void StateExit()
        {
            // this.networkAnimator.Animator.SetBool("Sprint", false);
            lerpTimeToSprint=0;

        }

        public override void StateInput()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            this.playerRef.Move(x,y);
            float z = Input.GetAxis("Sprint1");
            lerpTimeToSprint += Time.deltaTime;
            lerpTimeToSprint = Mathf.Clamp(lerpTimeToSprint, 0, 1);
            float lerp = Mathf.Lerp(playerRef.move.z, 2, easeOutQuart(lerpTimeToSprint));
            this.networkAnimator.Animator.SetFloat("X", this.playerRef.move.x*2);
            this.networkAnimator.Animator.SetFloat("Y", lerp);
        }

        public override void StateUpdate()
        {
            StateInput();


            // this.networkAnimator.Animator.SetFloat("Speed", this.playerRef.sprintFactor);
            
            this.networkAnimator.Animator.SetBool("Sprint", true);
            if (!playerRef.isGrounded)
            {
                stateMachineController.SetState("Falling");
                return;

            }
            if (Input.GetKeyDown(KeyCode.Mouse0)||Input.GetKeyDown(KeyCode.Mouse1))
            {
                stateMachineController.SetState("Movement");
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                stateMachineController.SetState("Movement");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stateMachineController.SetState("Jump");
                return;
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                stateMachineController.SetState("Sliding");
                return;
            }


        }
        public override void StatePhysicsUpdate()
        {

        }
        public override void StateLateUpdate()
        {
            this.playerRef.RotatePlayer();

        }
        float easeOutQuart(float time) {
            return 1 - Mathf.Pow(1 - time, 4);
        }
        float LerpToMovement(int Start, int End,ref float  lerpTime)
        {
             float lerp = Mathf.Lerp(Start, End, lerpTime);
             return lerpTime;
        }

    }
}
