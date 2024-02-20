using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationRiggingController : MonoBehaviour
{
    
    public Rig animationRigging;
    public PlayerStatsController playerStatsController;
    public Transform handLTarget;
    public Transform currentHandLTarget;
    [Header("IK Constraints")]
    TwoBoneIKConstraint twoBoneIKConstraint;


    private void Start()
    {
        playerStatsController.OnWeaponChanged += OnWeaponChanged;
        handLTarget.position = playerStatsController.weaponNoBuildGripPoint.position;

    }

    void Update()
    {
        if (!playerStatsController.hasPlayerSelectedBuild)
        {
            handLTarget.position = playerStatsController.weaponNoBuildGripPoint.position;
            return;
        }
        if (currentHandLTarget != null)
        {
            handLTarget.position = currentHandLTarget.position;
        }
        
    }
    
    public void OnWeaponChanged(Transform gripPoint)
    {
        if (playerStatsController.playerBuildSelected.first_weapon.weapon.weaponObjectController != null)
        {
            currentHandLTarget= playerStatsController.currentWeaponSelected.weaponObjectController.weaponGripPoint;
        }
        else
        {
            Debug.LogError("Weapon Object Controller is null");
        }
    }
}
