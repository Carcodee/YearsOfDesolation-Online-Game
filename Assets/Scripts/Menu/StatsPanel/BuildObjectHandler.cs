using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.StatsPanel
{
    public class BuildObjectHandler : MonoBehaviour
    {
        [HideInInspector]
        public BuildType buildType;
        public PlayerBuild playerBuildObjectSelected;
        
        public WeaponTemplate first_weapon_Template;
        public WeaponTemplate second_weapon_Template;
        
        
        public Image first_weapon_Image;
        public Image second_weapon_Image;
        
        [HideInInspector]
        public Image BorderImage;
        public Image [] upgradesSocketsImages;
        
        public PlayerStatsController playerStatsController;
        private void Start()
        {
            InitiliseBuild();
        }
        public void InitiliseBuild()
        {
            if (buildType == BuildType.AK_Pistol)
            {
                playerBuildObjectSelected = new AK_PistolBuild(first_weapon_Template, second_weapon_Template);
            }
            else if (buildType == BuildType.AK_Rifle)
            {
                // playerBuildObjectSelected = new AK_RifleBuild(first_weapon_Template, second_weapon_Template);
            }
            else if (buildType == BuildType.AK_Shotgun)
            {
                // playerBuildObjectSelected = new AK_ShotgunBuild(first_weapon_Template, second_weapon_Template);
            }
            

        }
        public void SelectBuild()
        {
            // playerStatsController.SetPlayerBuild(playerBuildObjectSelected);
        }
        public void UpgradeOrBuyBuild(int weaponIndex)
        {
            playerBuildObjectSelected.Upgrade(weaponIndex,  playerStatsController.GetAvaliblePoints());
            //animation later
        }
    }

}

public enum BuildType
{
    AK_Pistol,
    AK_Rifle,
    AK_Shotgun
}
