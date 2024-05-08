using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationRiggingController : MonoBehaviour, INetObjectToClean
{
    public bool shutingDown { get; set; }
    public static Action<float> OnAimOffsetChanged;
    public Rig animationRigging;
    public PlayerController playerStatsController;
    public Transform handLTarget;
    public Transform currentHandLTarget;
    [Header("IK Constraints")]
    public TwoBoneIKConstraint twoBoneIKConstraint;
    public MultiAimConstraint multiAimConstraint;

    private MultiAimConstraintData bakedAimOffsetData;
    private MultiAimConstraintData bakedNoAimOffsetData;
    
    
    private void Start()
    {
        playerStatsController.playerStats.OnWeaponChanged += OnWeaponChanged;
        handLTarget.position = playerStatsController.playerStats.weaponNoBuildGripPoint.position;

        bakedNoAimOffsetData = multiAimConstraint.data;
        bakedNoAimOffsetData.offset =Vector3.zero;
        bakedNoAimOffsetData.constrainedXAxis = true;
        bakedNoAimOffsetData.constrainedYAxis = true;
        bakedNoAimOffsetData.constrainedZAxis = true;
        
        Vector3 aimOffset = new Vector3(-50.0f, 0.0f, 0.0f);

        bakedAimOffsetData= multiAimConstraint.data;
        bakedAimOffsetData.offset = aimOffset;
        
    }

    void Update()
    {

        if (shutingDown)return;
        if ((playerStatsController.stateMachineController.networkAnimator.Animator.GetFloat("Aiming")>0.1) || playerStatsController.isShooting)
        {
            // Debug.Log("aiming");
            multiAimConstraint.data=bakedAimOffsetData;
        }else
        {
            multiAimConstraint.data = bakedNoAimOffsetData;
        }
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
        if (playerStatsController.playerStats.currentWeaponSelected.weaponObjectController.useTwoBoneIK)
        {
           twoBoneIKConstraint.weight = 1;
        }
        else if (!playerStatsController.playerStats.currentWeaponSelected.weaponObjectController.useTwoBoneIK)
        {
            twoBoneIKConstraint.weight = 0;
        }
        
        
    }


    public void SetRotationOffset(Vector3 offset)
    {
        var data = multiAimConstraint.data;
        data.offset =Vector3.zero;
        data.constrainedXAxis = true;
        multiAimConstraint.data = data;
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

    public void CleanData()
    {
        playerStatsController.playerStats.OnWeaponChanged -= OnWeaponChanged;
    }

    public void OnSpawn()
    {
    }

}
