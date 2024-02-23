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
            playerRef.sprintFactor = 1f;
            playerRef.move = Vector3.zero;
            playerRef.playerStats.SetHealth(playerRef.playerStats.GetMaxHealth());
        }

        public override void StateExit()
        {
            //respawn
            playerRef.sprintFactor = 1f;
            this.playerRef.ActivatePlayer();
            playerRef.playerStats.OnStatsChanged?.Invoke();
            // if (playerRef.playerStats.GetMaxHealth()!=playerRef.playerStats.GetHealth())
            // {
            //     playerRef.playerStats.health.OnValueChanged?.Invoke(0,playerRef.playerStats.GetMaxHealth());
            // }
            CanvasController.OnUpdateUI?.Invoke();
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
