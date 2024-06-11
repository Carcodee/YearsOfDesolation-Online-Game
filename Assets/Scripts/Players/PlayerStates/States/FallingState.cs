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
        private float xJumpDrag = 0.3f;
        private float yJumpDrag = 0.3f;
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
            float x= Input.GetAxis("Horizontal")*xJumpDrag;
            float y= Input.GetAxis("Vertical")*yJumpDrag;
            this.playerRef.Move(moveDir.x +x,moveDir.z+ y);
        }

        public override void StateUpdate()
        {
            StateInput();
            airTime += Time.deltaTime;

            if ((Input.GetKeyDown(KeyCode.Mouse1)&&!playerRef.hasPlaned && airTime>playerRef.airTimeToPlane) || 
                (Input.GetKeyDown(KeyCode.Mouse1)&&stateMachineController.lastStateName== "Jump"&&!playerRef.hasPlaned))
            {
                stateMachineController.SetState("Jetpack");
                return;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                stateMachineController.SetState("Aiming");
            }

            if (Input.GetKeyDown(KeyCode.Space)&& IsAlmostAtGround())
            {
                hasJumpAlmostAtTheGroud = true;
            }
            if (playerRef.isFallingGrounded())
            {
                if (hasJumpAlmostAtTheGroud)
                {
                    stateMachineController.SetState("Jump");
                    return;
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    stateMachineController.SetState("Sprint");
                    playerRef.playerStats.playerSoundController.PlayMovementSound(playerRef.playerStats.playerSoundController.landSound); 
                    playerRef.playerStats.playerVFXController.SpawnStepVfx(0);
                    playerRef.playerStats.playerVFXController.SpawnStepVfx(1);
                    networkAnimator.Animator.Play("Movement");
                    playerRef._bodyVelocity= Vector3.zero;
                    return;
                }
                playerRef.playerStats.playerSoundController.PlayMovementSound(playerRef.playerStats.playerSoundController.landSound);
                playerRef.playerStats.playerVFXController.SpawnStepVfx(0);
                playerRef.playerStats.playerVFXController.SpawnStepVfx(1);
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
            playerRef.ApplyGravity();
            playerRef.RotatePlayer();

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
