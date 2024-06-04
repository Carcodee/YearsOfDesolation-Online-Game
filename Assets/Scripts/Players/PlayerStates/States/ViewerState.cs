using Cinemachine;
using Demo_Project;
using UnityEngine;

namespace Players.PlayerStates.States
{
    public class ViewerState :PlayerStateBase
    {
        public ViewerState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;

        }

        bool isBattleRoyale;

        private bool playerSoundPlayed = false;
    
        public override void StateEnter()
        {
            // if (GameManager.Instance.isOnTutorial)
            // {
            //    playerRef.playerStats.GoMenuOnDead(); 
            // }
 
            GameController.instance.PlayerDeadForeverServerRpc(playerRef.OwnerClientId);
            playerSoundPlayed = false;
            playerRef.lockShoot = true;
            playerRef.sprintFactor = 1f;
            playerRef.move = Vector3.zero;
            playerRef.playerComponentsHandler.canvasController.HideUI();
            Cinemachine3rdPersonFollow thirdPersonFollow=playerRef.playerComponentsHandler.cinemachineVirtualCameraInstance.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            thirdPersonFollow.ShoulderOffset = new Vector3(0.62f, 1.0f, 0.0f);
            PlayerVFXController.OnDeadEffectHandle.CreateVFX(playerRef.playerStats.deadPosition, playerRef.transform.rotation, playerRef.IsServer);
            if (playerRef.playerStats.instigatorName!=string.Empty)
            {
                playerRef.playerComponentsHandler.canvasController.DeadNotification();
                playerRef.playerStats.instigatorName = "";
            }
        }
        
        public override void StateExit()
        {
            //respawn
            playerRef.playerStats.OnStatsChanged?.Invoke();
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
            CheckPlayerViewerChange();
        }

        public void CheckPlayerViewerChange()
        {
            PlayerStatsController playerKillerRef = playerRef.playerStats.playerControllerKillerRef.playerStats;
            if (playerKillerRef.health.Value<=0)
            {
                playerRef.playerStats.playerComponentsHandler.setViewer(playerKillerRef.clientIdInstigator.Value);
            }
        }

    }
}
