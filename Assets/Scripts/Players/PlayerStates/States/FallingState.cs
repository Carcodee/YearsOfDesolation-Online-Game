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
        bool hasJumpAlmostAtTheGroud = false;
        
        public override void StateEnter()
        {
            
            networkAnimator.Animator.SetBool("Fall",true);
            networkAnimator.Animator.SetBool("Land",false);
            playerRef._bodyVelocity.y = 0;
            moveDir = playerRef.move;
            this.playerRef.gravityMultiplier = 1;
            airTime = 0;
            hasJumpAlmostAtTheGroud = false;

        }

        public override void StateExit()
        {
            //animation
            
            networkAnimator.Animator.SetBool("Fall",false);
            networkAnimator.Animator.SetBool("Land",true);
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

            if ((Input.GetKeyDown(KeyCode.LeftAlt)&&!playerRef.hasPlaned && airTime>playerRef.airTimeToPlane) || 
                (Input.GetKeyDown(KeyCode.LeftAlt)&&stateMachineController.lastStateName== "Jump"&&!playerRef.hasPlaned))
            {
                stateMachineController.SetState("Jetpack");
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                stateMachineController.SetState("Aiming");
            }

            if (playerRef.isGrounded&& Input.GetKey(KeyCode.LeftShift))
            {
                stateMachineController.SetState("Sprint");
                networkAnimator.Animator.Play("Movement");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space)&& IsAlmostAtGround())
            {
                hasJumpAlmostAtTheGroud = true;
            }
            if (playerRef.isGrounded)
            {
                if (hasJumpAlmostAtTheGroud)
                {
                    stateMachineController.SetState("Jump");
                    return;
                }
                stateMachineController.SetState("Movement");
                playerRef._bodyVelocity= Vector3.zero;
                return;
            }

        }
        public override void StatePhysicsUpdate()
        {

        }
        public override void StateLateUpdate()
        {
            playerRef.RotatePlayer();
            playerRef.ApplyGravity();

        }

        public bool IsAlmostAtGround()
        {
            if (Physics.Raycast(playerRef.transform.position + playerRef.sphereOffset,-playerRef.transform.up,out RaycastHit hit, playerRef.maxDistanceToJumpAgain, playerRef.GroundLayer))
            {
                Debug.Log("Ground detected");
                return true;
            }
            return false;
        }
    }
}
