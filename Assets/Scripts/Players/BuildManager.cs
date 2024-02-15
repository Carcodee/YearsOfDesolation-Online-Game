using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Reflection;
using Object = UnityEngine.Object;

[System.Serializable]
public class PlayerBuild
{

    public WeaponItem first_weapon;
    public WeaponItem second_weapon;
    public int totalPrice=0;

    public string buildDescription;
    public string buildName;

    public UpgradableTypes[] upgrades;
    public UpgradableTypes[] upgradesBuffer;

    
    
    public PlayerBuild(WeaponTemplate first_weapon, WeaponTemplate second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);



        
    }


    public virtual void  Upgrade<T>(int index,UpgradeType value, T newValue)
    {
        upgradesBuffer[index].AddStats(value, newValue);
            
    }

    public void CreateDataBuild()
    {
        upgradesBuffer = new UpgradableTypes[2];
        upgrades = new UpgradableTypes[2];
        upgrades[0] = new UpgradableTypes();
        upgrades[1] = new UpgradableTypes();
        upgradesBuffer[0] = new UpgradableTypes();
        upgradesBuffer[1] = new UpgradableTypes();
        upgrades[0].weapon = this.first_weapon;
        upgrades[1].weapon = this.second_weapon;
        upgradesBuffer[0].weapon = this.first_weapon;
        upgradesBuffer[1].weapon = this.second_weapon;
        
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].shootRate =this.first_weapon.weapon.shootRate; ;
            upgrades[i].reloadSpeed =this.first_weapon.weapon.ammoBehaviour.reloadTime;
            upgrades[i].clipSize =this.first_weapon.weapon.ammoBehaviour.totalBullets;
            upgrades[i].recoil =this.first_weapon.weapon.minShootRefraction;
            upgradesBuffer[i] = upgrades[i];
        }
    }
    public void SetUpgrades()
    {

        upgrades = upgradesBuffer;
        totalPrice = 0;
    }
     


}

public class AK_PistolBuild : PlayerBuild {

    public AK_PistolBuild(WeaponTemplate first_weapon, WeaponTemplate second_weapon) : base(first_weapon, second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);
    }


}

public class SMG_HeavyPistol : PlayerBuild {

    public SMG_HeavyPistol(WeaponTemplate first_weapon, WeaponTemplate second_weapon) : base(first_weapon, second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);
    }
    
}

public class Shootgun_DoublePistols : PlayerBuild {

    public Shootgun_DoublePistols(WeaponTemplate first_weapon, WeaponTemplate second_weapon) : base(first_weapon, second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);
    }
  
}
public enum UpgradeType
{
    FireRate,
    ReloadSpeed,
    ClipSize,
    recoil,
}
[System.Serializable]
public struct StatTier<T>
{
    public T statValue;
    public int tier;
    public int price=>tier;
    public UpgradeType upgradeType;
}
[System.Serializable]
public class UpgradableTypes
{
    public WeaponItem weapon;
    public StatTier<float> shootRate;
    public StatTier<float> reloadSpeed;
    public StatTier<int> clipSize;
    public StatTier<float> recoil;
    // public UpgradableTypes(UpgradableTypes other)
    // {
    //     this.weapon = other.weapon;
    //     this.shootRate = other.shootRate;
    //     this.reloadSpeed = other.reloadSpeed;
    //     this.clipSize = other.clipSize;
    //     this.recoil = other.recoil;
    //     
    //     // For deep copy, manually copy all reference type fields
    // }
    public void AddStats<T>(UpgradeType value, T statValue)
    {
        switch (value)
        {
            case UpgradeType.FireRate:
                shootRate.statValue += (float) (object) statValue;
                recoil.tier++;
                break;
            case UpgradeType.ReloadSpeed:
                reloadSpeed.statValue += (float) (object) statValue;
                recoil.tier++;
                break;
            case UpgradeType.ClipSize:
                clipSize.statValue += (int) (object) statValue;
                recoil.tier++;
                break;
            case UpgradeType.recoil:
                recoil.statValue += (float) (object) statValue;
                recoil.tier++;
                break;
        }
        
    }
    public int ReturnTierFromType(UpgradeType value)
    {
        switch (value)
        {
            case UpgradeType.FireRate:
                return shootRate.tier;
                break;
            case UpgradeType.ReloadSpeed:
                return reloadSpeed.tier;
                break;
            case UpgradeType.ClipSize:
                return clipSize.tier;
                break;
            case UpgradeType.recoil:
                return recoil.tier;
                break;
        }
        return 0;
    }
    public void SetShootRate(float value)
    {
        shootRate.statValue = value;
    }
    public void SetReloadSpeed(float value)
    {
        reloadSpeed.statValue = value;
    }
    public void SetClipSize(int value)
    {
        clipSize.statValue = value;
    }
    public void SetRecoil(float value)
    {
        recoil.statValue = value;
    }
    
    
}