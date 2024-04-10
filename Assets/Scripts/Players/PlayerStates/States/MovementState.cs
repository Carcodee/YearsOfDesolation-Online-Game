using UnityEngine;

namespace Players.PlayerStates.States
{
    [System.Serializable]
    public class MovementState : PlayerStateBase
    {
        public MovementState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
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
            
            float x= Input.GetAxis("Horizontal");
            float y= Input.GetAxis("Vertical");
            Vector2 moveInputAtEnter = new Vector2(x, y);
            if (moveInputAtEnter.magnitude>0.1f)
            {
                networkAnimator.Animator.Play("Movement");
            }
            
        }

        public override void StateExit()
        {

        }

        public override void StateInput()
        {
            float x= Input.GetAxis("Horizontal");
            float y= Input.GetAxis("Vertical");

            animInput=new Vector2(x/1.5f , y/1.5f);

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


    
        
            this.networkAnimator.Animator.SetFloat("X", animInput.x);
            this.networkAnimator.Animator.SetFloat("Y", animInput.y);
            this.networkAnimator.Animator.SetFloat("Speed",  this.playerRef.sprintFactor);


        }
        public override void StatePhysicsUpdate()
        {

        }
        public override void StateLateUpdate()
        {
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
