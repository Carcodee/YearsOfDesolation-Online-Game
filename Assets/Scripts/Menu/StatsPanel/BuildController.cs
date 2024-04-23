using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildController : MonoBehaviour
{
    [HideInInspector]
    public PlayerStatsController playerStatsController;
    
    public TextMeshProUGUI BuildName;


    public WeaponObject[] weaponObjects;

    public TextMeshProUGUI totalPrice;


    public Color noSelectedColor;
    public Color selectedColor;
    public Color previewColor;
    public void Update()
    {
        totalPrice.text = (playerStatsController.playerBuildSelected.totalPrice * 10).ToString()+ "$";        
    }
    public void InitilizeBuild()
    {
        BuildName.text = playerStatsController.playerBuildSelected.buildName;

        weaponObjects[0].weaponImage.sprite = playerStatsController.playerBuildSelected.first_weapon.weapon.weaponImage;
        weaponObjects[0].weaponTitle.text = playerStatsController.playerBuildSelected.first_weapon.weapon.weaponName;
        weaponObjects[0].weapon = playerStatsController.playerBuildSelected.first_weapon;
        
        weaponObjects[1].weaponImage.sprite = playerStatsController.playerBuildSelected.second_weapon.weapon.weaponImage;
        weaponObjects[1].weaponTitle.text = playerStatsController.playerBuildSelected.second_weapon.weapon.weaponName;
        weaponObjects[1].weapon = playerStatsController.playerBuildSelected.second_weapon;

        F_In_F_Out_Obj.OnBuildSelected?.Invoke();
        F_In_F_Out_Obj.OnFadeInSkillElements?.Invoke();
        DisplaySlots();
        StatsDisplayer.OnReloadUpated?.Invoke();
    }
    public void Upgrade (StatsTierUpgrades upgrades)
    {
        
        if (upgrades.upgradeType == UpgradeType.ClipSize)
        {
            int value = (int)upgrades.value.intValue;
            playerStatsController.playerBuildSelected.Upgrade(upgrades.index, upgrades.upgradeType, value);
        }
        else
        {
            float value = upgrades.value.floatValue;
            playerStatsController.playerBuildSelected.Upgrade(upgrades.index, upgrades.upgradeType, value);
        }

        PreviewSlots();

    }

    public void Buy()
    {
        if (playerStatsController.GetAvaliblePoints() < playerStatsController.playerBuildSelected.totalPrice)
        {
            Cancel();
            Debug.Log("Not enough money");
            return;
        }
        playerStatsController.SetAvaliblePointsServerRpc(playerStatsController.GetAvaliblePoints() - playerStatsController.playerBuildSelected.totalPrice);
        playerStatsController.playerBuildSelected.SetUpgrades();
        DisplaySlots();
        F_In_F_Out_Obj.OnSetElementsWithWeapon_1?.Invoke();
        F_In_F_Out_Obj.OnSetElementsWithWeapon_2?.Invoke();
        F_In_F_Out_Obj.OnBuyAccepted?.Invoke();
        if (GameManager.Instance.isOnTutorial)
        {
            TutorialStagesHandler.instance.SetTutorialStage(TutorialStage.EnemyKill);
        }
    }

    public void Cancel() {
        for (int i = 0; i < weaponObjects.Length; i++) {
            for (int j = 0; j < weaponObjects[i].slotObjectController.Length; j++) {
                weaponObjects[i].slotObjectController[j].slotsToPreview =
                    playerStatsController.playerBuildSelected.upgradesBuffer[i].ReturnTierFromType(weaponObjects[i].slotObjectController[j].upgradeType);
                    weaponObjects[i].slotObjectController[j].FillSlots(weaponObjects[i].slotObjectController[j].currentSlotsUnlocked,
                    weaponObjects[i].slotObjectController[j].slotsToPreview, 
                    noSelectedColor);
                weaponObjects[i].slotObjectController[j].slotsToPreview = weaponObjects[i].slotObjectController[j].currentSlotsUnlocked;
                
            }
        }
        playerStatsController.playerBuildSelected.restartBuffer();
        StatsDisplayer.OnReloadUpated?.Invoke();
        F_In_F_Out_Obj.OnBuyDecliend?.Invoke();
        // Debug.Log(("ReloadTier: "+ playerStatsController.playerBuildSelected.upgrades[0].reloadSpeed.tier));
        // Debug.Log(("RecoilTier: " + playerStatsController.playerBuildSelected.upgrades[0].recoil.tier));
        // Debug.Log(("shootrateTier: " + playerStatsController.playerBuildSelected.upgrades[0].shootRate.tier));
        // Debug.Log(("clipsizeTier: " + playerStatsController.playerBuildSelected.upgrades[0].clipSize.tier));
    }
    public void DisplaySlots()
    {
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            for (int j = 0; j < weaponObjects[i].slotObjectController.Length; j++)
            {
                weaponObjects[i].slotObjectController[j].currentSlotsUnlocked=
                    playerStatsController.playerBuildSelected.upgrades[i].ReturnTierFromType(weaponObjects[i].slotObjectController[j].upgradeType);
                weaponObjects[i].slotObjectController[j].FillSlots(0,weaponObjects[i].slotObjectController[j].currentSlotsUnlocked,selectedColor);
                Debug.Log("Slots: "+ weaponObjects[i].slotObjectController[j].currentSlotsUnlocked);
            }    
        }
    }

    public void PreviewSlots()
    {
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            for (int j = 0; j < weaponObjects[i].slotObjectController.Length; j++)
            {
                weaponObjects[i].slotObjectController[j].slotsToPreview=
                    playerStatsController.playerBuildSelected.upgradesBuffer[i].ReturnTierFromType(weaponObjects[i].slotObjectController[j].upgradeType);
                weaponObjects[i].slotObjectController[j].FillSlots(weaponObjects[i].slotObjectController[j].currentSlotsUnlocked, weaponObjects[i].slotObjectController[j].slotsToPreview, previewColor);
            }    
        }
        StatsDisplayer.OnReloadUpated?.Invoke();
        F_In_F_Out_Obj.OnFadeInStatsElementsWeapon_1?.Invoke();
        F_In_F_Out_Obj.OnFadeInStatsElementsWeapon_2?.Invoke();
    }
    [System.Serializable]
    public struct WeaponObject
    {
        public WeaponItem weapon;
        public SlotObjectController [] slotObjectController;
        public Image weaponImage;
        public TextMeshProUGUI weaponTitle;
    }
}
