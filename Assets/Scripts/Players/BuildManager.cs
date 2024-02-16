using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Reflection;
using Object = UnityEngine.Object;
using System.Diagnostics;

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
       totalPrice += upgradesBuffer[index].AddStats(value, newValue);
            
    }

    
    public void restartBuffer() {
        upgradesBuffer[0].copy(upgrades[0]);
        upgradesBuffer[1].copy(upgrades[1]);
        totalPrice = 0;
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
        upgradesBuffer[0].weapon = this.first_weapon;
        
        upgrades[1].weapon = this.second_weapon;
        upgradesBuffer[1].weapon = this.second_weapon;

        upgrades[0].shootRate =this.first_weapon.weapon.shootRate; 
        upgrades[0].reloadSpeed =this.first_weapon.weapon.ammoBehaviour.reloadTime;
        upgrades[0].clipSize =this.first_weapon.weapon.ammoBehaviour.totalBullets;
        upgrades[0].recoil =this.first_weapon.weapon.minShootRefraction;
        upgradesBuffer[0].copy(upgrades[0]);
        
        upgrades[1].shootRate =this.second_weapon.weapon.shootRate; 
        upgrades[1].reloadSpeed =this.second_weapon.weapon.ammoBehaviour.reloadTime;
        upgrades[1].clipSize =this.second_weapon.weapon.ammoBehaviour.totalBullets;
        upgrades[1].recoil =this.second_weapon.weapon.minShootRefraction;
        upgradesBuffer[1].copy(upgrades[1]);
    }
    public void SetUpgrades()
    {
        // for (int i = 0; i < upgrades.Length; i++)
        // {
        //     upgrades[i].copy(upgradesBuffer[i]);
        //     first_weapon.weapon.shootRate = upgrades[i].shootRate;
        //     first_weapon.weapon.ammoBehaviour.reloadTime = upgrades[i].reloadSpeed;
        //     first_weapon.weapon.ammoBehaviour.totalBullets = upgrades[i].clipSize;
        //     first_weapon.weapon.minShootRefraction = upgrades[i].recoil;
        // }
        upgrades[0].copy(upgradesBuffer[0]);
        first_weapon.weapon.shootRate = upgrades[0].shootRate;
        first_weapon.weapon.ammoBehaviour.reloadTime = upgrades[0].reloadSpeed;
        first_weapon.weapon.ammoBehaviour.totalBullets = upgrades[0].clipSize;
        first_weapon.weapon.minShootRefraction = upgrades[0].recoil;
        
        upgrades[1].copy(upgradesBuffer[1]);
        second_weapon.weapon.shootRate = upgrades[1].shootRate;
        second_weapon.weapon.ammoBehaviour.reloadTime = upgrades[1].reloadSpeed;
        second_weapon.weapon.ammoBehaviour.totalBullets = upgrades[1].clipSize;
        second_weapon.weapon.minShootRefraction = upgrades[1].recoil;
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
public class Knives_DoublePistols : PlayerBuild {

    public Knives_DoublePistols(WeaponTemplate first_weapon, WeaponTemplate second_weapon) : base(first_weapon, second_weapon) {
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
    public int tierLimit = 5;

    //public UpgradableTypes() {;
    //}
    //public UpgradableTypes(UpgradableTypes other) {
    //    this.weapon = other.weapon;
    //    this.shootRate = other.shootRate;
    //    this.reloadSpeed = other.reloadSpeed;
    //    this.clipSize = other.clipSize;
    //    this.recoil = other.recoil;

    //}
    public void copy(UpgradableTypes other) {
        this.weapon = other.weapon;
        this.shootRate = other.shootRate;
        this.reloadSpeed = other.reloadSpeed;
        this.clipSize = other.clipSize;
        this.recoil = other.recoil;

    }
    public int AddStats<T>(UpgradeType value, T statValue)
    {

        switch (value)
        {
            case UpgradeType.FireRate:
                if (shootRate.tier >= tierLimit) { return 0; }
                shootRate.statValue += (float) (object) statValue;
                shootRate.tier++;
                return shootRate.price;

            case UpgradeType.ReloadSpeed:
                if (reloadSpeed.tier >= tierLimit) { return 0; }
                reloadSpeed.statValue += (float) (object) statValue;
                reloadSpeed.tier++;
                return reloadSpeed.price;

            case UpgradeType.ClipSize:
                if (clipSize.tier>= tierLimit) { return 0; }
                clipSize.statValue += (int) (object) statValue;
                clipSize.tier++;
                return clipSize.price;

            case UpgradeType.recoil:
                if (recoil.tier >= tierLimit) { return 0; }
                recoil.statValue += (float) (object) statValue;
                recoil.tier++;
                return recoil.price;

        }
        return 0;
        
    }

    public int ReturnTierFromType(UpgradeType value)
    {
        switch (value)
        {
            case UpgradeType.FireRate:
                return shootRate.tier;
            case UpgradeType.ReloadSpeed:
                return reloadSpeed.tier;
            case UpgradeType.ClipSize:
                return clipSize.tier;
            case UpgradeType.recoil:
                return recoil.tier;
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