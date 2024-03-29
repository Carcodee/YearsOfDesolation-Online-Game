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
        PreviewStatsDeserializer(ref previewWeaponStats,buildController.weaponObjects[weaponIndex]);
        currentWeaponStats.InitializePreviews();
        
        LinkDataToUI(currentWeaponStats, currentStats);
        currentStats.BaseDamage.text = baseDamage.ToString("0.0");       
        previewStats.BaseDamage.gameObject.SetActive(false);
        
        if (currentWeaponStats.CompareValues(previewWeaponStats))
        {
            previewStats.gameObject.SetActive(false);
            return;
        }
        previewStats.gameObject.SetActive(true);
        
        previewWeaponStats.SetPreviews(currentWeaponStats);
        LinkDataToUI(previewWeaponStats, previewStats);
        // CompareUIStrings(currentStats, previewStats);
        
        
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
            if (clipSizeSlotsDiff<0)clipSizeSlotsDiff=0;
            weaponToLink.clipSize.value= weaponToLink.clipSize.value + (clipSizeSlotsDiff * clipSizeUpgrade.value.intValue);
       }

       if (weaponToLink.fireRate.slotObjectController!=null)
       {
            int fireRateSlotsDiff= weaponToLink.fireRate.slotObjectController.slotsToPreview-weaponToLink.fireRate.slotObjectController.currentSlotsUnlocked;
            if(fireRateSlotsDiff<0)fireRateSlotsDiff=0;
            weaponToLink.fireRate.value= weaponToLink.fireRate.value + (fireRateSlotsDiff * fireRateUpgrade.value.floatValue);
       }

       if (weaponToLink.reloadSpeed.slotObjectController!=null)
       {
            int reloadSpeedSlotsDiff= weaponToLink.reloadSpeed.slotObjectController.slotsToPreview-weaponToLink.reloadSpeed.slotObjectController.currentSlotsUnlocked;
            if(reloadSpeedSlotsDiff<0)reloadSpeedSlotsDiff=0;
            weaponToLink.reloadSpeed.value= weaponToLink.reloadSpeed.value + (reloadSpeedSlotsDiff * reloadSpeedUpgrade.value.floatValue);

       }

       if (weaponToLink.recoilAmount.slotObjectController!=null)
       {
            int recoilAmountSlotsDiff= weaponToLink.recoilAmount.slotObjectController.slotsToPreview-weaponToLink.recoilAmount.slotObjectController.currentSlotsUnlocked;
            if(recoilAmountSlotsDiff<0)recoilAmountSlotsDiff=0;
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

        layoutGroup.clipSize.text = stats.clipSize.value.ToString("0.0");
        layoutGroup.clipSize.gameObject.SetActive(stats.clipSize.isPreview);
        
        layoutGroup.fireRate.text = stats.fireRate.value.ToString("0.0");
        layoutGroup.fireRate.gameObject.SetActive(stats.fireRate.isPreview);
        
        layoutGroup.reloadSpeed.text = stats.reloadSpeed.value.ToString("0.0");
        layoutGroup.reloadSpeed.gameObject.SetActive(stats.reloadSpeed.isPreview);
        
        layoutGroup.recoilAmount.text = (stats.recoilAmount.value*10000).ToString("0.0");
        layoutGroup.recoilAmount.gameObject.SetActive(stats.recoilAmount.isPreview);
    }
    public void CompareUIStrings(TextLayoutGroup current, TextLayoutGroup preview)
    {
        if (current.clipSize.text == preview.clipSize.text)
        {
            preview.clipSize.gameObject.SetActive(false);
        }
        if (current.fireRate.text == preview.fireRate.text)
        {
            preview.fireRate.gameObject.SetActive(false);
        }
        if (current.reloadSpeed.text == preview.reloadSpeed.text)
        {
            preview.reloadSpeed.gameObject.SetActive(false);
        }
        if (current.recoilAmount.text == preview.recoilAmount.text)
        {
            preview.recoilAmount.gameObject.SetActive(false);
        }
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
            if ((int)(100*reloadSpeed.value) ==(int)(100* other.reloadSpeed.value) &&
                (int)(100*fireRate.value) == (int)(100*other.fireRate.value) &&
                (int)clipSize.value == (int)other.clipSize.value &&
                (int)recoilAmount.value*100000 == (int)other.recoilAmount.value*100000)
            {
                return true;
            }
            
           
            return false;
        }

        public void InitializePreviews()
        {
            reloadSpeed.isPreview = true;
            fireRate.isPreview = true;
            clipSize.isPreview = true;
            recoilAmount.isPreview = true;
        }
        public void SetPreviews(WeaponStats other)
        {
            reloadSpeed.isPreview = true;
            fireRate.isPreview = true;
            clipSize.isPreview = true;
            recoilAmount.isPreview = true;
            if ((int)(100*reloadSpeed.value) ==(int)(100* other.reloadSpeed.value))
            {
               reloadSpeed.isPreview = false; 
            }
            if ((int)(100*fireRate.value) == (int)(100*other.fireRate.value))
            {
                fireRate.isPreview = false;
            }
            if ((int)clipSize.value == (int)other.clipSize.value)
            {
                clipSize.isPreview = false;
            }
            if ((int)recoilAmount.value*100000 == (int)other.recoilAmount.value*100000)
            {
                recoilAmount.isPreview = false;
            }
            
                
        }
    }
   
    [System.Serializable]
    public struct UpgradeData
    {
        public SlotObjectController slotObjectController;
        public UpgradeType upgradeType;
        public bool isPreview;
        public float value;

    }
}
