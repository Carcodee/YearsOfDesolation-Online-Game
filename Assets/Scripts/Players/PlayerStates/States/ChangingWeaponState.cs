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
            playerRef.sprintFactor = 1;
            playerRef.hasPlaned = false;
            networkAnimator.Animator.SetFloat("Aiming", 0);
            playerRef.playerStats.playerComponentsHandler.cinmachineCloseLookCameraIntance.Priority = 5;
            playerRef.lockShoot = true;
            layerIndex = networkAnimator.Animator.GetLayerIndex(weaponToChange.weapon.changeWeaponAnimation.LayerName);
            oldWeapon = playerRef.playerStats.currentWeaponSelected;
            playerRef.playerStats.SetWeapon(weaponToChange);
            
            networkAnimator.Animator.Play(weaponToChange.weapon.changeWeaponAnimation.weaponChange);
            
            MyUtilities.SetDefaultUpperLayer(networkAnimator.Animator, weaponToChange.weapon.changeWeaponAnimation.LayerName, 
                oldWeapon.weapon.changeWeaponAnimation.LayerName);
            
            networkAnimator.Animator.GetLayerIndex(weaponToChange.weapon.changeWeaponAnimation.LayerName);
            playerRef.playerStats.currentWeaponSelected = weaponToChange;

        }

        public override void StateExit()
        {
            // playerRef.playerStats.currentWeaponSelected = weaponToChange;
            playerRef.lockShoot = false;
            if(isAimingInAtLastFrame)
            {
                playerRef.playerStats.playerComponentsHandler.cinmachineCloseLookCameraIntance.Priority = 20;
            }


        }

        public override void StateLateUpdate()
        {
            if (!MyUtilities.IsAnimationPlaying(networkAnimator.Animator, weaponToChange.weapon.changeWeaponAnimation.weaponChange,layerIndex))
            {
                stateMachineController.SetChangingWeaponState(weaponToChange, "OnWeapon");
                return;
            }
            playerRef.playerStats.playerComponentsHandler.cinmachineCloseLookCameraIntance.Priority = 5;
            isAimingInAtLastFrame = Input.GetKey(KeyCode.Mouse1);   
        }

        public override void StateInput()
        {
            float x= Input.GetAxis("Horizontal");
            float y= Input.GetAxis("Vertical");
            if (playerRef.playerComponentsHandler.IsPlayerLocked())
            {
                x = 0;
                y = 0;
            }
            animInput=new Vector2(x * this.playerRef.moveAnimationSpeed, y * this.playerRef.moveAnimationSpeed);

            this.playerRef.Move(x, y);
        }

        public override void StatePhysicsUpdate()
        {
            this.playerRef.RotatePlayer();

        }

        public override void StateUpdate()
        {
            StateInput();
            this.networkAnimator.Animator.SetFloat("X", animInput.x);
            this.networkAnimator.Animator.SetFloat("Y", animInput.y);
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