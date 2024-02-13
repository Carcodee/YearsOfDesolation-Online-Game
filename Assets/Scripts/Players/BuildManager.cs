using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerBuild
{

    public WeaponItem first_weapon;
    public WeaponItem second_weapon;
    public int currentBuildUpgrade=0;
    public int currentBuildUpgradeCost=0;

    public string buildDescription;
    public string buildName;
    

    
    public PlayerBuild(WeaponTemplate first_weapon, WeaponTemplate second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);
    }


    public virtual void Upgrade(int weaponIdex, int MoneyEarned) {
    }



}

public class AK_PistolBuild : PlayerBuild {

    public AK_PistolBuild(WeaponTemplate first_weapon, WeaponTemplate second_weapon) : base(first_weapon, second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);
    }
    public override void Upgrade(int weaponIdex, int MoneyEarned) {
        if (currentBuildUpgrade < currentBuildUpgradeCost) {
            Debug.Log("Not enough money to upgrade weapon");
            return;
        }
        Debug.Log("Upgrading weapon " + weaponIdex);
        switch (weaponIdex) {

            case 0:
                //upgrade first weapon
                break;
            case 1:
                //upgrade second weapon
                break;
            default:
                break;
        }

    }
}

public class Sniper_Pistol : PlayerBuild {

    public Sniper_Pistol(WeaponTemplate first_weapon, WeaponTemplate second_weapon) : base(first_weapon, second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);
    }
    public override void Upgrade(int weaponIdex, int MoneyEarned) {
        if (currentBuildUpgrade < currentBuildUpgradeCost) {
            Debug.Log("Not enough money to upgrade weapon");
            return;
        }
        Debug.Log("Upgrading weapon " + weaponIdex);
        switch (weaponIdex) {

            case 0:
                //upgrade first weapon
                break;
            case 1:
                //upgrade second weapon
                break;
            default:
                break;
        }

    }
}

public class Shootgun_DoublePistols : PlayerBuild {

    public Shootgun_DoublePistols(WeaponTemplate first_weapon, WeaponTemplate second_weapon) : base(first_weapon, second_weapon) {
        this.first_weapon =new WeaponItem(first_weapon) ;
        this.second_weapon = new WeaponItem(second_weapon);
    }
    public override void Upgrade(int weaponIdex, int MoneyEarned) {
        if (currentBuildUpgrade < currentBuildUpgradeCost) {
            Debug.Log("Not enough money to upgrade weapon");
            return;
        }
        Debug.Log("Upgrading weapon " + weaponIdex);
        switch (weaponIdex) {

            case 0:
                //upgrade first weapon
                break;
            case 1:
                //upgrade second weapon
                break;
            default:
                break;
        }

    }
}