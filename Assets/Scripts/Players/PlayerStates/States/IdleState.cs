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
        private bool isOnMovement= false;
        public override void StateEnter()
        {
            base.StateEnter();
            playerRef.hasPlaned = false;
            playerRef.sprintFactor = 1;
            isOnMovement = false;
            playerRef.ActivateAim(0);
            playerRef.playerComponentsHandler.ActivateCamera(TutorialManager.instance.playerRef.playerComponentsHandler.cinmachineCloseLookCameraIntance, false, 5);
            if (!playerRef.isGrounded)
            {
                networkAnimator.Animator.SetBool("Fall",true);
            }
            else
            {
                networkAnimator.Animator.SetBool("Fall",false);
                networkAnimator.Animator.Play("Movement");
                isOnMovement = true;
            }
            this.networkAnimator.Animator.SetFloat("X", 0);
            this.networkAnimator.Animator.SetFloat("Y", 0);
            this.networkAnimator.Animator.SetFloat("Speed",  this.playerRef.sprintFactor);
            networkAnimator.Animator.SetBool("Sprint", false);
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

            Debug.Log("Idling");
            this.networkAnimator.Animator.SetFloat("X", 0);
            this.networkAnimator.Animator.SetFloat("Y", 0);
            this.networkAnimator.Animator.SetFloat("Speed",  this.playerRef.sprintFactor);


        }
        public override void StatePhysicsUpdate()
        {
            playerRef.ApplyGroundGravity();
        }
        public override void StateLateUpdate()
        {
            if (!playerRef.isGrounded)
            {
                this.playerRef.ApplyGravity();
            }
            else if(!isOnMovement && playerRef.isGrounded)
            {
                networkAnimator.Animator.SetBool("Fall",false);
                networkAnimator.Animator.Play("Movement");
                isOnMovement = true;
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