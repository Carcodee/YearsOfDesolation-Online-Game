using UnityEngine;

namespace Players.PlayerStates.States
{
    public class FallingState : PlayerStateBase
    {
        public FallingState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;

        }
        Vector3 moveDir;
        float airTime;
        public override void StateEnter()
        {
            
            networkAnimator.Animator.SetBool("Fall",true);
            playerRef._bodyVelocity.y = 0;
            moveDir = playerRef.move;
            this.playerRef.gravityMultiplier = 1;
            airTime = 0;


        }

        public override void StateExit()
        {
            //animation
            networkAnimator.Animator.SetBool("Fall",false);
            airTime = 0;

        }

        public override void StateInput()
        {
            float x= Input.GetAxis("Horizontal")*0.3f;
            float y= Input.GetAxis("Vertical")*0.3f;
            this.playerRef.Move(moveDir.x +x,moveDir.z+ y);
        }

        public override void StateUpdate()
        {
            StateInput();
            airTime += Time.deltaTime;

            if ((Input.GetKeyDown(KeyCode.Space)&&!playerRef.hasPlaned && airTime>playerRef.airTimeToPlane) || 
                (Input.GetKeyDown(KeyCode.Space)&&stateMachineController.lastStateName== "Jump"&&!playerRef.hasPlaned))
            {
                stateMachineController.SetState("Jetpack");
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                stateMachineController.SetState("Aiming");
            }


        }
        public override void StatePhysicsUpdate()
        {

        }
        public override void StateLateUpdate()
        {
            playerRef.RotatePlayer();
            playerRef.ApplyGravity();
            if (playerRef.isGrounded&& Input.GetKey(KeyCode.LeftShift))
            {
                stateMachineController.SetState("Sprint");
                networkAnimator.Animator.Play("Movement");
                return;
            }
            if (playerRef.isGrounded)
            {
                stateMachineController.SetState("Movement");
                return;
            }
        }

    }
}
