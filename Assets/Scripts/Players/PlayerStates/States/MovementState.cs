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
            networkAnimator.Animator.Play("Movement");
            // networkAnimator.Animator.SetFloat("Aiming", 0);

        }

        public override void StateExit()
        {

        }

        public override void StateInput()
        {
            float x= Input.GetAxis("Horizontal");
            float y= Input.GetAxis("Vertical");
            animInput=new Vector2(Input.GetAxis("Horizontal") * this.playerRef.moveAnimationSpeed, Input.GetAxis("Vertical") * this.playerRef.moveAnimationSpeed);
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
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                stateMachineController.SetState("Sprint");
                return;
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
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
        }
    }
}
