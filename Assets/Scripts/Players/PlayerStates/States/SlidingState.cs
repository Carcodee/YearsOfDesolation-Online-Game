using UnityEngine;

namespace Players.PlayerStates.States
{
    public class SlidingState : PlayerStateBase
    {
        public SlidingState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;
        }
        public float slidingTimer;
        public float slidingTime=1.4f;
        private float slidingTimeNormalized;
        private Vector3 moveDir;
    
        public override void StateEnter()
        {
            base.StateEnter();
            networkAnimator.Animator.SetBool("Sliding", true);

            playerRef.sprintFactor = 2.0f;
            moveDir = playerRef.move;
            this.networkAnimator.Animator.Play("Slide");

            
        }

        public override void StateExit()
        {
            networkAnimator.Animator.SetBool("Sliding", false);
        }

        public override void StateInput()
        {
            slidingTimeNormalized = slidingTimer / slidingTime;
            float speedFactor= Mathf.Lerp(2.0f, 1.0f, slidingTimeNormalized);
            playerRef.sprintFactor = speedFactor;
        }
 

        public override void StateUpdate()
        {
            StateInput();
            if (Input.GetKeyUp(KeyCode.Space))
            {
                stateMachineController.SetState("Jump");
                return;
            }
            this.networkAnimator.Animator.SetFloat("X", moveDir.x);
            this.networkAnimator.Animator.SetFloat("Y", moveDir.z);
            slidingTimer += Time.deltaTime;


            if (slidingTimer > slidingTime)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    slidingTimer = 0;
                    stateMachineController.SetState("Sprint");
                    // networkAnimator.Animator.Play("Movement");
                    return;
                }
                slidingTimer = 0;
                stateMachineController.SetState("Movement");
            }
            if (!playerRef.isGrounded)
            {
                stateMachineController.SetState("Falling");
                return;

            }
        }
        public override void StateLateUpdate()
        {
            playerRef.ApplyGravity();
            playerRef.RotatePlayer();
        }
        public override void StatePhysicsUpdate()
        {

        }
    }



    public class CrouchState : PlayerStateBase
    {
        public CrouchState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;
        }
        public override void StateEnter()
        {
            playerRef.sprintFactor = 0.5f;

        }

        public override void StateExit()
        {
            networkAnimator.Animator.SetBool("Crouch", false);
        }


        public override void StateInput()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            this.playerRef.Move(x, y);
        }
        public override void StateUpdate()
        {
            StateInput();
            if (!playerRef.isGrounded)
            {
                stateMachineController.SetState("Falling");
                return;

            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                stateMachineController.SetState("Movement");
                return;
            }  
            this.networkAnimator.Animator.SetFloat("X", playerRef.move.x);
            this.networkAnimator.Animator.SetFloat("Y", playerRef.move.z);
            networkAnimator.Animator.SetBool("Crouch", true);



        }
        public override void StatePhysicsUpdate()
        {

        }
        public override void StateLateUpdate()
        {
            this.playerRef.RotatePlayer();
        }
    }
}