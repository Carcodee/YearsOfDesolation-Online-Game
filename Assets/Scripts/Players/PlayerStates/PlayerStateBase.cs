using Unity.Netcode.Components;

namespace Players.PlayerStates
{
    public class PlayerStateBase : StateMachineBase
    {
        public PlayerController playerRef;
        public NetworkAnimator networkAnimator;

        public PlayerStateBase(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;
        }
        public override void StateEnter()
        {

        
        }

        public override void StateExit()
        {
        
        }

        public override void StateLateUpdate()
        {
        
        }
        public virtual void StateInput()
        {

        }
        public override void StatePhysicsUpdate()
        {
        
        }

        public override void StateUpdate()
        {
        
        }
    }
}



