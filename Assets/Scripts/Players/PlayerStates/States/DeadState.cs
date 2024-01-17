using UnityEngine;

namespace Players.PlayerStates.States
{
    public class DeadState :PlayerStateBase
    {
        public DeadState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;

        }

        bool isBattleRoyale;
        float currentRespawnTimer;
    
        public override void StateEnter()
        {
            currentRespawnTimer= 0;
        
        }

        public override void StateExit()
        {
            this.playerRef.ActivatePlayer();
            playerRef.playerStats.SetHealth(playerRef.playerStats.GetMaxHealth());
            playerRef.playerStats.OnStatsChanged?.Invoke();
            //respawn
        }

        public override void StateLateUpdate()
        {
        
        }

        public override void StateInput()
        {
        
        }

        public override void StatePhysicsUpdate()
        {
        }

        public override void StateUpdate()
        {
            currentRespawnTimer += Time.deltaTime;
            Debug.Log("Time to respawn: "+GameController.instance.respawnTime);
        
            if (currentRespawnTimer>GameController.instance.respawnTime)
            {
                stateMachineController.SetState("Movement");
            }
        }


    }
}
