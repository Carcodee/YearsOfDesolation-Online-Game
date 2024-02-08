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
        public WeaponItem weapontToChange;
        WeaponItem oldWeapon;
        Vector2 animInput;
        public override void StateEnter()
        {
            playerRef.sprintFactor = 1;
            playerRef.hasPlaned = false;

            playerRef.lockShoot = true;
            layerIndex = networkAnimator.Animator.GetLayerIndex(weapontToChange.weapon.changeWeaponAnimation.LayerName);
            oldWeapon = playerRef.playerStats.currentWeaponSelected;
            playerRef.playerStats.SetWeapon(weapontToChange);
            
            networkAnimator.Animator.Play(weapontToChange.weapon.changeWeaponAnimation.weaponChange);
            
            MyUtilities.SetDefaultUpperLayer(networkAnimator.Animator, weapontToChange.weapon.changeWeaponAnimation.LayerName, 
                oldWeapon.weapon.changeWeaponAnimation.LayerName);
            
            networkAnimator.Animator.GetLayerIndex(weapontToChange.weapon.changeWeaponAnimation.LayerName);
        }

        public override void StateExit()
        {
            playerRef.playerStats.currentWeaponSelected = weapontToChange;
            playerRef.lockShoot = false;

        }

        public override void StateLateUpdate()
        {
            if (!MyUtilities.IsAnimationPlaying(networkAnimator.Animator, weapontToChange.weapon.changeWeaponAnimation.weaponChange,layerIndex))
            {
                stateMachineController.SetState("Movement");
            }
            
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
