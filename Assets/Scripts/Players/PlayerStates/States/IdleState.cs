using UnityEngine;

namespace Players.PlayerStates.States
{
    [System.Serializable]
    public class IdleState : PlayerStateBase
    {
        public IdleState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;
        }
        Vector2 animInput;
        public override void StateEnter()
        {
            base.StateEnter();
            playerRef.hasPlaned = false;
            playerRef.sprintFactor = 1;
            networkAnimator.Animator.SetBool("Fall",false);
        }

        public override void StateExit()
        {

        }

        public override void StateInput()
        {
            this.playerRef.Move(0, 0);
        }

        public override void StateUpdate()
        {
            StateInput();

            if (!playerRef.isGrounded)
            {
                stateMachineController.SetState("Falling");
                return;

            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                stateMachineController.SetState("Aiming");

            }
            if (Input.GetKeyDown(KeyCode.LeftShift) && playerRef.move.magnitude > 0.1f)
            {
                stateMachineController.SetState("Sprint");
                return;
            } 
            
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                stateMachineController.SetState("Crouch");
                return;

            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stateMachineController.SetState("Jump");
                return;
            }


    
        
            this.networkAnimator.Animator.SetFloat("X", 0);
            this.networkAnimator.Animator.SetFloat("Y", 0);
            this.networkAnimator.Animator.SetFloat("Speed",  this.playerRef.sprintFactor);


        }
        public override void StatePhysicsUpdate()
        {

        }
        public override void StateLateUpdate()
        {
            if (!playerRef.isGrounded)
            {
                this.playerRef.ApplyGravity();
            }
            this.playerRef.RotatePlayer();
            if (MyUtilities.IsThisAnimationPlaying(networkAnimator.Animator, "StepLeft", 0)&&playerRef.isRotating)
            {
            }
            else
            {
                playerRef.isRotating = false;
                networkAnimator.Animator.SetBool("RotatingLeft", false);
                
            }
            if (MyUtilities.IsThisAnimationPlaying(networkAnimator.Animator, "StepRight", 0)&&playerRef.isRotating)
            {
            }
            else
            {
                playerRef.isRotating = false;
                networkAnimator.Animator.SetBool("RotatingRight", false);
            }
        }
        
    }
}