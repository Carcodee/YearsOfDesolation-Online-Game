using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationRiggingController : MonoBehaviour
{
    
    public Rig animationRigging;
    public PlayerController playerStatsController;
    public Transform handLTarget;
    public Transform currentHandLTarget;
    [Header("IK Constraints")]
    public TwoBoneIKConstraint twoBoneIKConstraint;
    public MultiAimConstraint multiAimConstraint;


    private void Start()
    {
        playerStatsController.playerStats.OnWeaponChanged += OnWeaponChanged;
        handLTarget.position = playerStatsController.playerStats.weaponNoBuildGripPoint.position;
    }

    void Update()
    {
        
        if (playerStatsController.isSprinting)
        {
            multiAimConstraint.weight = 0;
            twoBoneIKConstraint.weight = 0;
        }
        else
        {
            multiAimConstraint.weight = 1;
            twoBoneIKConstraint.weight = 1;
        }
        if (!playerStatsController.playerStats.hasPlayerSelectedBuild)
        {
            handLTarget.position = playerStatsController.playerStats.weaponNoBuildGripPoint.position;
            handLTarget.rotation = playerStatsController.playerStats.weaponNoBuildGripPoint.rotation;
            return;
        }
        if (currentHandLTarget != null)
        {
            handLTarget.position = currentHandLTarget.position;
            handLTarget.rotation = currentHandLTarget.rotation;

        }

        if (playerStatsController.playerStats.currentWeaponSelected==null)
        {
           return;   
        }
        Debug.Log(playerStatsController.playerStats.currentWeaponSelected.weaponObjectController.useTwoBoneIK);
        if (playerStatsController.playerStats.currentWeaponSelected.weaponObjectController.useTwoBoneIK)
        {
           twoBoneIKConstraint.weight = 1;
        }
        else if (!playerStatsController.playerStats.currentWeaponSelected.weaponObjectController.useTwoBoneIK)
        {
            twoBoneIKConstraint.weight = 0;
        }
        
        
    }
    
    public void OnWeaponChanged(Transform gripPoint)
    {
        if (playerStatsController.playerStats.playerBuildSelected.first_weapon.weapon.weaponObjectController != null)
        {
            currentHandLTarget= playerStatsController.playerStats.currentWeaponSelected.weaponObjectController.weaponGripPoint;
        }
        else
        {
            Debug.LogError("Weapon Object Controller is null");
        }
    }
}
