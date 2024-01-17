using UnityEngine;

namespace Players.PlayerStates.States
{
    public class JetpackState : PlayerStateBase
    {
        public JetpackState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;

        }
        Vector3 moveDir;
        private float aimAnimation;

        public override void StateEnter()
        {
            networkAnimator.Animator.SetBool("Fall", true);
            playerRef._bodyVelocity.y = 0;
            moveDir = playerRef.move;
            this.playerRef.gravityMultiplier = 0.05f;
            aimAnimation = 0;

        }
    
        public override void StateExit()
        {
            //animation
            playerRef.hasPlaned = true;

        }

        public override void StateInput()
        {

        }

        public override void StateUpdate()
        {
            this.playerRef.AimAinimation(ref aimAnimation, networkAnimator);
            this.playerRef.Shoot();
            this.playerRef.Reloading();
        
            if (Input.GetKeyUp(KeyCode.Space))
            {
                stateMachineController.SetState("Falling");
            }
        }
        public override void StatePhysicsUpdate()
        {
            playerRef.ApplyGravity();
            if (playerRef.isGrounded)
            {
                stateMachineController.SetState("Movement");
            }
        }
        public override void StateLateUpdate()
        {
        }
    
    }
}
