using UnityEngine;

namespace Players.PlayerStates.States
{
    public class JumpState : PlayerStateBase
    {
        public JumpState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;
        }
        Vector3 moveDir;
        Vector3 moveDirAirForce;

        public override void StateEnter()
        {
            base.StateEnter();
            moveDir = playerRef.move;
            this.playerRef.Jump();
            this.playerRef.gravityMultiplier = 1;
            networkAnimator.Animator.Play("Jump");
        
        }

        public override void StateExit()
        {
            //animation


        }

        public override void StateInput()
        {
            //moveDirAirForce = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        }

        public override void StateUpdate()
        {
            StateInput();
            if (Input.GetKey(KeyCode.Mouse1))
            {
                this.networkAnimator.Animator.SetFloat("X", this.playerRef.move.x);
                this.networkAnimator.Animator.SetFloat("Y", this.playerRef.move.z);
                //this.playerRef.AimAinimation(ref aimAnimation,networkAnimator);
                networkAnimator.Animator.SetFloat("Aiming", 1);
            }
            if (playerRef._bodyVelocity.y < 0)
            {
                stateMachineController.SetState("Falling");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stateMachineController.SetState("Jetpack");
            }


        }
        public override void StatePhysicsUpdate()
        {
            playerRef.ApplyGravity();
        }
        public override void StateLateUpdate()
        {

            playerRef.RotatePlayer();

        }

    }
}
