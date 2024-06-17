using Demo_Project;
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

        private bool playerSoundPlayed = false;
    
        public override void StateEnter()
        {
            // if (GameManager.Instance.isOnTutorial)
            // {
            //    playerRef.playerStats.GoMenuOnDead(); 
            // }
            playerSoundPlayed = false;
            playerRef.lockShoot = true;
            currentRespawnTimer = 0;
            playerRef.sprintFactor = 1f;
            playerRef.move = Vector3.zero;
            playerRef.playerStats.SetHealth(playerRef.playerStats.GetMaxHealth());
            playerRef.playerStats.playerSoundController.PlayActionSound(playerRef.playerStats.playerSoundController.DeathSound);
            PlayerVFXController.OnDeadEffectHandle.CreateVFX(playerRef.playerStats.deadPosition, playerRef.transform.rotation, playerRef.IsServer);
            
            if (playerRef.playerStats.instigatorName!=string.Empty)
            {
                playerRef.playerComponentsHandler.canvasController.DeadNotification();
                playerRef.playerStats.instigatorName = "";
            }
            if (GameManager.Instance.isOnTutorial)
            {
               TutorialStagesHandler.instance.SetTutorialStage(TutorialStage.ZoneComing);
            }
        }
        
        public override void StateExit()
        {
            //respawn
            playerRef.sprintFactor = 1f;
            this.playerRef.ActivatePlayer();
            playerRef.playerStats.OnStatsChanged?.Invoke();
            CanvasController.OnUpdateUI?.Invoke();
            playerRef.lockShoot = false;
            playerRef.playerStats.playerSoundController.PlayActionSound(playerRef.playerStats.playerSoundController.playerRespawnedSound, 1.0f, 1.0f, true);
            if (playerRef.playerStats.hasPlayerSelectedBuild)
            {
                playerRef.playerStats.SetMainWeapon();
                playerRef.playerStats.stateMachineController.networkAnimator.Animator.SetLayerWeight(1, 1.0f);
                playerRef.playerStats.stateMachineController.networkAnimator.Animator.SetLayerWeight(2, 0.0f);
            }
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
            if (currentRespawnTimer>=(GameController.instance.respawnTime-playerRef.playerStats.playerSoundController.respawningSound.length) && !playerSoundPlayed)
            {
                playerRef.playerStats.playerSoundController.PlayActionSound(playerRef.playerStats.playerSoundController.respawningSound, 1.0f, 1.0f);
                playerSoundPlayed = true;
            }
        
            if (currentRespawnTimer>GameController.instance.respawnTime)
            {

                stateMachineController.SetState("Movement");

            }
        }

    }
}
