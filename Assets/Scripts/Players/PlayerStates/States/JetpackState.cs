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
        private float currentJetpackTime = 0;
        public override void StateEnter()
        {
            currentJetpackTime = 0.0f;
            networkAnimator.Animator.SetBool("Fall", true);
            networkAnimator.Animator.SetBool("Land", false);
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
            currentJetpackTime += Time.deltaTime;
            this.playerRef.Reloading();
        
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                stateMachineController.SetState("Falling");
                playerRef._bodyVelocity= Vector3.zero;
            }
        }
        public override void StatePhysicsUpdate()
        {
            playerRef.ApplyGravity();
            if (IsAlmostAtGround() || currentJetpackTime>playerRef.jetpackTime)
            {
                playerRef._bodyVelocity.y = 0;
                stateMachineController.SetState("Falling");
            }
        }
        public override void StateLateUpdate()
        {
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
