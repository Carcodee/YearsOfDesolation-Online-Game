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
            playerRef.lockShoot = true;
            playerRef.sprintFactor = 1.6f;
            playerRef.isSprinting = true;
            this.networkAnimator.Animator.SetBool("Sprint", playerRef.isSprinting);

        }

        public override void StateExit()
        {
            // this.networkAnimator.Animator.SetBool("Sprint", false);
            lerpTimeToSprint=0;
            playerRef.lockShoot =false;
            playerRef.isSprinting = false;
            this.networkAnimator.Animator.SetBool("Sprint", playerRef.isSprinting);

        }

        public override void StateInput()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            this.playerRef.Move(x,y);
            float z = Input.GetAxis("Sprint1");
            lerpTimeToSprint += Time.deltaTime;
            lerpTimeToSprint = Mathf.Clamp(lerpTimeToSprint, 0, 1);
            float targetDirY= (y<0)? -2*Mathf.Abs(y):2*Mathf.Abs(y);
            float targetDirX= (x<0)? -2*Mathf.Abs(x):2*Mathf.Abs(x);
            float lerpX = Mathf.Lerp(playerRef.move.x,targetDirX, easeOutQuart(lerpTimeToSprint));
            float lerpY = Mathf.Lerp(playerRef.move.z,targetDirY, easeOutQuart(lerpTimeToSprint));
            this.networkAnimator.Animator.SetFloat("X", lerpX);
            this.networkAnimator.Animator.SetFloat("Y", lerpY);
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
