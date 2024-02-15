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

    public TextMeshProUGUI currentMoney;

    public WeaponObject[] weaponObjects;

    public TextMeshProUGUI totalPrice;
    
    public void Update()
    {
        currentMoney.text = (playerStatsController.GetAvaliblePoints()*10).ToString()+ "$";
        totalPrice.text = (playerStatsController.playerBuildSelected.totalPrice * 10).ToString()+ "$";        
    }
    public void InitilizeBuild()
    {
        BuildName.text = playerStatsController.playerBuildSelected.buildName;
        currentMoney.text = (playerStatsController.GetAvaliblePoints()*10).ToString()+ "$";
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            weaponObjects[i].weaponImage.sprite = playerStatsController.playerBuildSelected.first_weapon.weapon.weaponImage;
            weaponObjects[i].weaponTitle.text = playerStatsController.playerBuildSelected.first_weapon.weapon.weaponName;
        }
        DisplaySlots();
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
        Debug.Log("Upgraded");
        
    }

    public void Buy()
    {
        if (playerStatsController.GetAvaliblePoints()<playerStatsController.playerBuildSelected.totalPrice)
        {
            Debug.Log("Not enough money");
            return;
        }
        playerStatsController.playerBuildSelected.SetUpgrades();
    }
    public void DisplaySlots()
    {
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            for (int j = 0; j < weaponObjects[i].slotObjectController.Length; j++)
            {
                weaponObjects[i].slotObjectController[j].currentSlotsUnlocked=
                    playerStatsController.playerBuildSelected.upgrades[i].ReturnTierFromType(weaponObjects[i].slotObjectController[j].upgradeType);
                weaponObjects[i].slotObjectController[j].FillSlots(weaponObjects[i].slotObjectController[j].currentSlotsUnlocked);
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
                weaponObjects[i].slotObjectController[j].FillSlots(weaponObjects[i].slotObjectController[j].currentSlotsUnlocked);
            }    
        }
    }
    [System.Serializable]
    public struct WeaponObject
    {
        public SlotObjectController [] slotObjectController;
        public Image weaponImage;
        public TextMeshProUGUI weaponTitle;
    }
}
