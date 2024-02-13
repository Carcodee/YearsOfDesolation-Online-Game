using System;
using System.Collections.Generic;
using Michsky.UI.Heat;
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
        
        [Header("References")]
        public Image [] upgradesSocketsImages;
        
        public PlayerStatsController playerStatsController;
        public ShopButtonManager shopButtonManager;

        private void Awake()
        {
            InitiliseBuild();

        }

        private void OnEnable()
        {
        }

        private void Start()
        {
        }
        public void InitiliseBuild()
        {
            string Description= playerBuildObjectSelected.buildDescription;
            string name = playerBuildObjectSelected.buildName;
            if (buildType == BuildType.AK_Pistol)
            {
                playerBuildObjectSelected = new AK_PistolBuild(first_weapon_Template, second_weapon_Template);
                
            }
            else if (buildType == BuildType.Sniper_Pistol)
            {
                playerBuildObjectSelected = new Sniper_Pistol(first_weapon_Template, second_weapon_Template);

            }
            else if (buildType == BuildType.Shotgun_DoublePistol)
            {
                 playerBuildObjectSelected = new Shootgun_DoublePistols(first_weapon_Template, second_weapon_Template);

            }
            playerBuildObjectSelected.buildDescription = Description;
            playerBuildObjectSelected.buildName = name;
            

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

        public void DisplayData()
        {
            shopButtonManager.buttonTitle = playerBuildObjectSelected.buildDescription;
            shopButtonManager.buttonDescription = playerBuildObjectSelected.buildName;
            Debug.Log("DisplayData");
            Debug.Log(playerBuildObjectSelected.buildDescription);
            Debug.Log(playerBuildObjectSelected.buildName);
            if (playerStatsController.GetLevel()<2)
            {
                shopButtonManager.priceText = "UNLOCK AT LEVEL 2";
                shopButtonManager.priceObj.color = Color.gray;
            }
            else
            {
                shopButtonManager.priceText = "SET BUILD";
                shopButtonManager.priceObj.color = Color.yellow;
            }
            shopButtonManager.UpdateUI();
            
            
        }
    }
}

public enum BuildType
{
    AK_Pistol,
    Sniper_Pistol,
    Shotgun_DoublePistol,
}
