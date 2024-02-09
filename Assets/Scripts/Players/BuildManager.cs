using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerBuild
{
    public WeaponTemplate first_weapon_Template;
    public WeaponTemplate second_weapon_Template;
    public WeaponItem first_weapon;
    public WeaponItem second_weapon;
    public int currentBuildUpgrade=0;
    public int currentBuildUpgradeCost=0;

    public PlayerBuild() {
        this.first_weapon = new WeaponItem(first_weapon_Template);
        this.second_weapon = new WeaponItem(second_weapon_Template);
    }

    
    public PlayerBuild(WeaponItem first_weapon, WeaponItem second_weapon) {
        this.first_weapon = first_weapon;
        this.second_weapon = second_weapon;
    }
    

    public virtual void Upgrade(int weaponIdex, int MoneyEarned) {
    }



}

public class AK_PistolBuild : PlayerBuild {



    public AK_PistolBuild() {
        this.first_weapon = new WeaponItem(first_weapon_Template);
        this.second_weapon = new WeaponItem(second_weapon_Template);
    }


    public AK_PistolBuild(WeaponItem first_weapon, WeaponItem second_weapon) {
        this.first_weapon = first_weapon;
        this.second_weapon = second_weapon;
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
