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
        private Vector3 moveDir;
    
        public override void StateEnter()
        {
            base.StateEnter();
            playerRef.sprintFactor = 2.0f;
            moveDir = playerRef.move;
            this.networkAnimator.Animator.Play("Slide");

        }

        public override void StateExit()
        { 
        }

        public override void StateInput()
        {

        }
 

        public override void StateUpdate()
        {
            StateInput();
            this.networkAnimator.Animator.SetFloat("X", moveDir.x);
            this.networkAnimator.Animator.SetFloat("Y", moveDir.z);
            slidingTimer += Time.deltaTime;
            if (slidingTimer > slidingTime)
            {
                slidingTimer = 0;
                stateMachineController.SetState("Movement");
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                stateMachineController.SetState("Jump");
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
            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                stateMachineController.SetState("Movement");
                return;
            }  
            this.networkAnimator.Animator.SetFloat("X", playerRef.move.x);
            this.networkAnimator.Animator.SetFloat("Y", playerRef.move.z);
            networkAnimator.Animator.SetBool("Crouch", true);

            if (Input.GetKey(KeyCode.Mouse1))
            {
                this.networkAnimator.Animator.SetFloat("X", this.playerRef.move.x);
                this.networkAnimator.Animator.SetFloat("Y", this.playerRef.move.z);
                //this.playerRef.AimAinimation(ref aimAnimation,networkAnimator);
                networkAnimator.Animator.SetFloat("Aiming", 1);
            }
            else
            {
                networkAnimator.Animator.SetFloat("Aiming", 0);
            }

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