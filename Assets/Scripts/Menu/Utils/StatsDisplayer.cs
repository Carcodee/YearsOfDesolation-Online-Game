using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StatsDisplayer : MonoBehaviour
{
    [Header("Logic")]
    public float baseDamage;
    public WeaponStats currentWeaponStats;
    public WeaponStats previewWeaponStats;
    public BuildController buildController;
    public WeaponItem weaponItem;
    public int weaponIndex;
    
    [Header("Current UI")]
    public TextMeshProUGUI weaponName;
    public TextLayoutGroup currentStats;
    public TextLayoutGroup previewStats;
    
    [Header("StartTier upgraders")]
    public StatsTierUpgrades clipSizeUpgrade;
    public StatsTierUpgrades fireRateUpgrade;
    public StatsTierUpgrades reloadSpeedUpgrade;
    public StatsTierUpgrades recoilAmountUpgrade;
    
    
    public static Action OnReloadUpated;
    void Start()
    {
        
    }

    private void OnEnable()
    {
        OnReloadUpated += UpdateUI; 
    }

    private void OnDisable()
    {
        OnReloadUpated -= UpdateUI;
    } 

    void Update()
    {
        
        
        
    }


    public void UpdateUI()
    {


        Debug.Log("Weapon item is null");
        weaponItem = buildController.weaponObjects[weaponIndex].weapon;
        currentWeaponStats = DeserializeWeaponData(weaponItem);
        previewWeaponStats = DeserializeWeaponData(weaponItem);

        LinkDataToUI(currentWeaponStats, currentStats);
        
        if (!currentWeaponStats.CompareValues(previewWeaponStats))
        {
            return;
        }
        
        PreviewStatsDeserializer(ref previewWeaponStats,buildController.weaponObjects[weaponIndex]);
        LinkDataToUI(previewWeaponStats, previewStats);
        
        
    }
    public SlotObjectController FindSlotType(UpgradeType type, SlotObjectController[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].upgradeType == type)
            {
                return slots[i];
            }
        }
        Debug.Log("Slot not found");
        return null;
    }


    public void PreviewStatsDeserializer(ref WeaponStats weaponToLink,BuildController.WeaponObject weaponObject)
    {
       for (int i = 0; i < weaponObject.slotObjectController.Length; i++)
       {
           if (weaponObject.slotObjectController[i].upgradeType == weaponToLink.clipSize.upgradeType)
           {
               weaponToLink.clipSize.slotObjectController = weaponObject.slotObjectController[i];
           }
           else if (weaponObject.slotObjectController[i].upgradeType == weaponToLink.fireRate.upgradeType)
           {
               weaponToLink.fireRate.slotObjectController = weaponObject.slotObjectController[i];
           }
           else if (weaponObject.slotObjectController[i].upgradeType == weaponToLink.reloadSpeed.upgradeType)
           {
                weaponToLink.reloadSpeed.slotObjectController = weaponObject.slotObjectController[i];
           }
           else if (weaponObject.slotObjectController[i].upgradeType == weaponToLink.recoilAmount.upgradeType)
           {
                weaponToLink.recoilAmount.slotObjectController = weaponObject.slotObjectController[i];
           }
       }


       if (weaponToLink.clipSize.slotObjectController!=null)
       {
            int clipSizeSlotsDiff= weaponToLink.clipSize.slotObjectController.slotsToPreview-weaponToLink.clipSize.slotObjectController.currentSlotsUnlocked;
            weaponToLink.clipSize.value= weaponToLink.clipSize.value + (clipSizeSlotsDiff * clipSizeUpgrade.value.floatValue);
           
       }

       if (weaponToLink.fireRate.slotObjectController!=null)
       {
            int fireRateSlotsDiff= weaponToLink.fireRate.slotObjectController.slotsToPreview-weaponToLink.fireRate.slotObjectController.currentSlotsUnlocked;
            weaponToLink.fireRate.value= weaponToLink.fireRate.value + (fireRateSlotsDiff * fireRateUpgrade.value.floatValue);
           
       }

       if (weaponToLink.reloadSpeed.slotObjectController!=null)
       {
            int reloadSpeedSlotsDiff= weaponToLink.reloadSpeed.slotObjectController.slotsToPreview-weaponToLink.reloadSpeed.slotObjectController.currentSlotsUnlocked;
            weaponToLink.reloadSpeed.value= weaponToLink.reloadSpeed.value + (reloadSpeedSlotsDiff * reloadSpeedUpgrade.value.floatValue);
       }

       if (weaponToLink.recoilAmount.slotObjectController!=null)
       {
            int recoilAmountSlotsDiff= weaponToLink.recoilAmount.slotObjectController.slotsToPreview-weaponToLink.recoilAmount.slotObjectController.currentSlotsUnlocked;
            weaponToLink.recoilAmount.value= weaponToLink.recoilAmount.value + (recoilAmountSlotsDiff * recoilAmountUpgrade.value.floatValue);
           
       }
       
       
       
       
    }
    
    public WeaponStats DeserializeWeaponData(WeaponItem weaponItemLinked)
    {
        WeaponStats stats = new WeaponStats();
        baseDamage = weaponItemLinked.weapon.weaponDamage;
        
        stats.clipSize.value = weaponItemLinked.weapon.ammoBehaviour.totalBullets.statValue;
        stats.clipSize.upgradeType = UpgradeType.ClipSize;
        stats.clipSize.slotObjectController = null;
        
        stats.fireRate.value = weaponItemLinked.weapon.shootRate.statValue;
        stats.fireRate.upgradeType = UpgradeType.FireRate;
        stats.fireRate.slotObjectController = null;
        
        stats.recoilAmount.value = weaponItemLinked.weapon.minShootRefraction.statValue;
        stats.recoilAmount.upgradeType = UpgradeType.recoil;
        stats.recoilAmount.slotObjectController = null;
        
        stats.reloadSpeed.value = weaponItemLinked.weapon.ammoBehaviour.reloadTime.statValue;
        stats.reloadSpeed.upgradeType = UpgradeType.ReloadSpeed;
        stats.reloadSpeed.slotObjectController = null;
        return stats;
    }
    
    public void LinkDataToUI(WeaponStats stats, TextLayoutGroup layoutGroup)
    {
        layoutGroup.BaseDamage.text = baseDamage.ToString("0.0");
        layoutGroup.clipSize.text = stats.clipSize.value.ToString("0.0");
        layoutGroup.fireRate.text = stats.fireRate.value.ToString("0.0");
        layoutGroup.reloadSpeed.text = stats.reloadSpeed.value.ToString("0.0");
        layoutGroup.recoilAmount.text = stats.recoilAmount.value.ToString("0.0");
    }
    [System.Serializable]
    public struct WeaponStats
    {
        public UpgradeData reloadSpeed;
        public UpgradeData fireRate;
        public UpgradeData clipSize;
        public UpgradeData recoilAmount;
        
        public bool CompareValues(WeaponStats other)
        {
            if (reloadSpeed.value == other.reloadSpeed.value && fireRate.value == other.fireRate.value &&
                clipSize.value == other.clipSize.value && recoilAmount.value == other.recoilAmount.value)
            {
                return true;
            }
            return false;
        }
    }
   
    [System.Serializable]
    public struct UpgradeData
    {
        public SlotObjectController slotObjectController;
        public UpgradeType upgradeType;
        public float value;
    }
}
