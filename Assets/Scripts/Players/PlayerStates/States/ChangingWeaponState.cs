using System.Collections;
using System.Collections.Generic;
using Players.PlayerStates;
using UnityEngine;

public class ChangingWeaponState : PlayerStateBase
{

        public ChangingWeaponState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
        {
            playerRef = stateMachineController.GetComponent<PlayerController>();
            networkAnimator = stateMachineController.networkAnimator;

        }

        bool isBattleRoyale;
        float currentRespawnTimer;
        int layerIndex;
        public WeaponItem weaponToChange;
        WeaponItem oldWeapon;
        Vector2 animInput;
        bool isAimingInAtLastFrame;
        public override void StateEnter()
        {
            // playerRef.sprintFactor = 1;
            playerRef.hasPlaned = false;
            networkAnimator.Animator.SetFloat("Aiming", 0);
            playerRef.playerStats.playerComponentsHandler.cinmachineCloseLookCameraIntance.Priority = 5;
            playerRef.lockShoot = true;
            layerIndex = networkAnimator.Animator.GetLayerIndex(weaponToChange.weapon.weaponAnimation.LayerName);
            oldWeapon = playerRef.playerStats.currentWeaponSelected;
            playerRef.playerStats.SetWeapon(weaponToChange);
            networkAnimator.Animator.Play(weaponToChange.weapon.weaponAnimation.weaponChange);
            
            playerRef.playerStats.playerSoundController.PlayWeaponSound(playerRef.playerStats.playerSoundController.weaponChangeStart, true);
            MyUtilities.SetDefaultUpperLayer(networkAnimator.Animator, weaponToChange.weapon.weaponAnimation.LayerName, 
                oldWeapon.weapon.weaponAnimation.LayerName);
            
            playerRef.playerStats.currentWeaponSelected = weaponToChange;

            playerRef.playerStats.currentlayerIndex = layerIndex;


        }

        public override void StateExit()
        {
            // playerRef.playerStats.currentWeaponSelected = weaponToChange;
            playerRef.lockShoot = false;
            playerRef.playerStats.playerSoundController.PlayWeaponSound(playerRef.playerStats.playerSoundController.weaponChangeEnd, true);
            playerRef.playerStats.currentWeaponSelected.weapon.shootTimer= playerRef.playerStats.currentWeaponSelected.weapon.shootRate.statValue+0.1f;
            if(isAimingInAtLastFrame)
            {
                playerRef.playerStats.playerComponentsHandler.cinmachineCloseLookCameraIntance.Priority = 20;
            }


        }

        public override void StateLateUpdate()
        {
            if (!MyUtilities.IsThisAnimationPlaying(networkAnimator.Animator, weaponToChange.weapon.weaponAnimation.weaponChange,layerIndex))
            {
                stateMachineController.SetChangingWeaponState(weaponToChange, "OnWeapon");
                return;
            }
            playerRef.playerStats.playerComponentsHandler.cinmachineCloseLookCameraIntance.Priority = 5;
            isAimingInAtLastFrame = Input.GetKey(KeyCode.Mouse1);   
        }

        public override void StateInput()
        {


        }

        public override void StatePhysicsUpdate()
        {
            this.playerRef.RotatePlayer();

        }

        public override void StateUpdate()
        {
            StateInput();

        }




}
public class OnWeaponState : PlayerStateBase
{

        public OnWeaponState(string name, StateMachineController stateMachineController) : base(name, stateMachineController)
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

        public override void StateInput()
        {
 
        }

        public override void StatePhysicsUpdate()
        {

        }

        public override void StateUpdate()
        {

        }




}