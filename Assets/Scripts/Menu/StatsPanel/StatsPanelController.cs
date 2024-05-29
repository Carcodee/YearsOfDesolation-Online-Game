using System;
using System.Threading.Tasks;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HorizontalSelector = Michsky.UI.Heat.HorizontalSelector;

namespace Menu.StatsPanel
{
    public class StatsPanelController : MonoBehaviour, INetObjectToClean
    {
        public bool shutingDown { get; set; }
        public static Action OnPanelClosed;
        public static Action OnPanelOpen;
        [Header("References")] [SerializeField]
        private PlayerStatsController playerStatsController;

        [SerializeField] private PlayerVFXController playerVFXController;
        public GameObject[] statsObjects;
        public WindowManager windowManager;
        public Animator panelAnimator;

        
        [Header("Stats")] private bool isRefreshedStats = false;
        private bool isPanelRefreshed = false;

        [Header("HeadStats")] public TextMeshProUGUI level;
        public TextMeshProUGUI avaliblePointsText;
        public Button openPannel;




        [Header("Build")] 
        AK_PistolBuild ak_PistolBuild;
        SMG_HeavyPistol smg_HeavyPistol;
        Shootgun_DoublePistols shootgun_DoublePistols;
        public BuildController buildObject;
        public GameObject buildSelector;       
        
        public BuildObjectPairs[] buildObjectsPairs;
        public HorizontalSelector horizontalBuildSelector;

        [Header("Sesion Variables")] [SerializeField]
        private int avaliblePoints;

        [SerializeField] private int sesionPoints;
        public bool isPanelOpen { get; private set; }

        [Header("Animation")] public float animationTime;
        public float animationSpeed;
        public float animationFunction => 1 - Mathf.Pow(1 - animationTime, 3);
        public Transform targetPos;
        public Vector3 endPos;
        public Vector3 startPos;

        [Header("Money")] 
        public TextMeshProUGUI currentMoney;
        public Animator moneyAnimator;


        public GameObject [] objectsToDeactivate;
        public bool hasBeenDeactivated;
        [Header("FadeIn_FadeOut")] 
        
        public F_In_F_Out_Obj[] f_In_F_Out_Obj;

        public bool Ready=false;
        void Start()
        {

            isPanelOpen = false;
            playerStatsController = GetComponentInParent<PlayerStatsController>();
            playerVFXController = playerStatsController.GetComponent<PlayerVFXController>();
            endPos = targetPos.localPosition;

            startPos = transform.localPosition;
            for (int i = 0; i < buildObjectsPairs.Length; i++)
            {
                buildObjectsPairs[i].InitialiseBuilds();
                buildObjectsPairs[i].SetReferences(playerStatsController);
                buildObjectsPairs[i].DisplayBuilds();
            }
            currentMoney.text = (playerStatsController.GetAvaliblePoints()*10).ToString()+ "$";

            playerStatsController.avaliblePoints.OnValueChanged+=AddMoneyAnimationOnPanel;
            OnPanelClosed += Deactivate;
            OnPanelOpen += ActivatePanel;
            WaitForSetup();
            INetObjectToClean[] objectToCleans = GetComponents<INetObjectToClean>();
            foreach (INetObjectToClean objectToClean in objectToCleans)
            {
                CleanerController.instance.AddObjectToList(objectToClean);
            }
        }
        public void AddMoneyAnimationOnPanel(int oldVal, int newVal)
        {
            currentMoney.text = (playerStatsController.GetAvaliblePoints()*10).ToString()+ "$";
            int moneyAdded = newVal - oldVal;
            moneyAnimator.Play("MoneyAddedOnPanel");

        }

        void Update()
        {
            if (shutingDown)return;
            if (Input.GetKeyDown(KeyCode.B))
            {
                isPanelOpen = !isPanelOpen;
                UpdateStats();
                AudioManager.instance.OpenShopSound();
                if (isPanelOpen)
                {
                    PostProccesingManager.instance.LerpActivateMenuBlur();
                    PlayerComponentsHandler.IsCurrentDeviceMouse = true;
                    isPanelRefreshed = false;
                    
                }
                else
                {
                    hasBeenDeactivated = true;
                    F_In_F_Out_Obj.OnFadeOutSkillElements?.Invoke();
                    PlayerComponentsHandler.IsCurrentDeviceMouse = false;
                    PostProccesingManager.instance.LerpDeactivateMenuBlur();
                    isPanelRefreshed = true;
                    if (!GameManager.Instance.isOnTutorial)return;
                    if (!TutorialStagesHandler.instance.currentStage.hasDialogFinished)PlayerComponentsHandler.IsCurrentDeviceMouse = true;

                }
                // HandlePanel();
            }

            if (isPanelOpen)
            {
                OpenPanel();
            }
            else
            {
                isRefreshedStats = false;
            }

            AnimatePanel();
            // HandleSelector();
        }

        public async void WaitForSetup()
        {
            await Task.Delay(400);
            gameObject.SetActive(false);
            Ready = true;
        }

        public void ActivatePanel()
        {
            isPanelOpen = true;
            PostProccesingManager.instance.LerpActivateMenuBlur();
            PlayerComponentsHandler.IsCurrentDeviceMouse = true;
            isPanelRefreshed = false;
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
        public void LoadBuildObject()
        {
            buildObject.gameObject.SetActive(true);
            new WaitForSeconds(0.1f);
            buildSelector.SetActive(false);
            buildObject.playerStatsController = playerStatsController;
            buildObject.InitilizeBuild();
        }
        public void AnimatePanel()
        {
            if (isPanelOpen && animationTime < 1)
            {
                animationTime += Time.deltaTime * animationSpeed;
                Mathf.Clamp(animationTime, 0, 1);
            }

            if (!isPanelOpen && animationTime > 0)
            {
                animationTime -= Time.deltaTime * animationSpeed;
                Mathf.Clamp(animationTime, 0, 1);
            }

            float xPos = Mathf.Lerp(startPos.x, endPos.x, animationFunction);
            transform.localPosition = new Vector3(xPos, transform.localPosition.y, 0);
            
            if (animationTime<=0)
            {
                OnPanelClosed.Invoke();
            }
        }



        public void OpenPanel()
        {
            if (isPanelRefreshed)
            {
                return;
            }

            if (playerStatsController.hasPlayerSelectedBuild)
            {
                
                F_In_F_Out_Obj.OnFadeInSkillElements?.Invoke();
                
            }
            avaliblePoints = playerStatsController.GetAvaliblePoints();
            avaliblePointsText.text = "Avalible Points: " + avaliblePoints.ToString();
            level.text = "Level: " + playerStatsController.GetLevel().ToString();
            sesionPoints = playerStatsController.GetAvaliblePoints();
            isPanelRefreshed = true;

        }

        public void SetBuildPanelPair()
        {
            for (int i = 0; i < buildObjectsPairs.Length; i++)
            {
                if (i == horizontalBuildSelector.index)
                {
                    
                    buildObjectsPairs[horizontalBuildSelector.index].gameObject.SetActive(true);
                    if (buildObjectsPairs[horizontalBuildSelector.index].IsInitialized()== false)
                    {
                        buildObjectsPairs[horizontalBuildSelector.index].InitialiseBuilds();
                        buildObjectsPairs[horizontalBuildSelector.index].SetReferences(playerStatsController);
                        buildObjectsPairs[horizontalBuildSelector.index].DisplayBuilds();

                    }
                    continue;
                }
                buildObjectsPairs[i].gameObject.SetActive(false);

            }
        }
        public void ClosePanel()
        {
            avaliblePoints = 0;
            sesionPoints = 0;
        }


        public void UpdateStats()
        {
            // LoadAllStats();
            avaliblePointsText.text = "Avalible Points: " + avaliblePoints.ToString();
            level.text = "Level: " + playerStatsController.GetLevel().ToString();
        }

        #region DEPRECATED

        // public void LoadAllStats()
        // {
        //     for (int i = 0; i < statValues.Length; i++)
        //     {
        //         LoadStat(i);
        //     }
        //     isRefreshedStats = true;
        //
        // }
        // public void LoadStat(int statType)
        // {
        //     if (statType> statValues.Length)
        //     {
        //         Debug.Log(statType+ "index Stat not found");
        //         return;            
        //     }
        //
        //     switch (statType)
        //     {
        //         case (int)StatType.reloadTime:
        //             statValues[statType].text = playerStatsController.GetHaste().ToString();
        //             break;
        //         case (int)StatType.health:
        //             statValues[statType].text = playerStatsController.GetMaxHealth().ToString();
        //             break;
        //         case (int)StatType.armor:
        //             statValues[statType].text = playerStatsController.GetArmor().ToString();
        //             break;
        //         case (int)StatType.damage:
        //             statValues[statType].text = playerStatsController.GetDamageDone().ToString();
        //             break;
        //         case (int)StatType.stamina:
        //             statValues[statType].text = playerStatsController.GetStamina().ToString();
        //             break;
        //     }
        //
        // }
        //     public void AddStat(int buttonType)
        //     {
        //
        //         if (avaliblePoints <= 0)
        //         {
        //             Debug.Log("No points");
        //             return;
        //         }
        //     
        //         switch (buttonType)
        //         {
        //             case (int)StatType.reloadTime:
        //                 playerStatsController.SetHasteServerRpc(playerStatsController.GetHaste() + 1);
        //                 playerVFXController.ApplyPointsEffect();
        //                 Debug.Log("AddStat");
        //                 avaliblePoints--;
        //                 break;
        //             case (int)StatType.health:
        //                 playerStatsController.SetMaxHealthServerRpc(playerStatsController.GetMaxHealth() + 1);
        //                 playerVFXController.ApplyPointsEffect();
        //                 avaliblePoints--;
        //                 break;
        //             case (int)StatType.armor:
        //                 playerStatsController.SetArmorServerRpc(playerStatsController.GetArmor() + 1);
        //                 playerVFXController.ApplyPointsEffect();
        //                 avaliblePoints--;
        //                 break;
        //             case (int)StatType.damage:
        //                 playerStatsController.SetDamageServerRpc(playerStatsController.GetDamageDone() + 1);
        //                 playerVFXController.ApplyPointsEffect();
        //                 avaliblePoints--;
        //                 break;
        //             case (int)StatType.stamina:
        //                 playerStatsController.SetStaminaServerRpc(playerStatsController.GetStamina() + 1);
        //                 playerVFXController.ApplyPointsEffect();
        //                 avaliblePoints--;
        //                 break;
        //         }
        //         Debug.Log("Reloaded");
        //         UpdateStats();
        //     
        //     }
        //
        //     public void RemoveStat(int buttonType)
        //     {
        //         switch (buttonType)
        //         {
        //             case (int)StatType.reloadTime:
        //                 if (playerStatsController.GetHaste() <= 1)
        //                 {
        //                     Debug.Log("Cant remove");
        //                     return;
        //                 }
        //                 playerStatsController.SetHasteServerRpc(playerStatsController.GetHaste() - 1);
        //                 Debug.Log("removed");
        //                 avaliblePoints++;
        //                 break;
        //         
        //             case (int)StatType.health:
        //                 if (playerStatsController.GetHealth() <= 1)
        //                 {
        //                     Debug.Log("Cant remove");
        //                     return;
        //                 }
        //                 playerStatsController.SetMaxHealthServerRpc(playerStatsController.GetMaxHealth() -1 );
        //
        //                 Debug.Log("removed");
        //                 avaliblePoints++;
        //                 break;
        //         
        //             case (int)StatType.armor:
        //                 if (playerStatsController.GetArmor() <= 1)
        //                 {
        //                     Debug.Log("Cant remove");
        //                     return;
        //                 }
        //                 playerStatsController.SetArmorServerRpc(playerStatsController.GetArmor() - 1);
        //                 avaliblePoints++;
        //                 break;
        //         
        //             case (int)StatType.damage:
        //                 if (playerStatsController.GetDamageDone() <= 1)
        //                 {
        //                     Debug.Log("Cant remove");
        //                     return;
        //                 }
        //                 playerStatsController.SetDamageServerRpc(playerStatsController.GetDamageDone() - 1);
        //                 avaliblePoints++;
        //                 break;
        //         
        //             case (int)StatType.stamina:
        //                 if (playerStatsController.GetStamina() <= 1)
        //                 {
        //                     Debug.Log("Cant remove");
        //                     return;
        //                 }
        //
        //                 playerStatsController.SetStaminaServerRpc(playerStatsController.GetStamina() - 1);
        //                 avaliblePoints++;
        //                 break;
        //         }
        //         Debug.Log("Reloaded");
        //         UpdateStats();
        //     
        //     }
        //
        //
        // }

        #endregion


        [Serializable]
        public enum StatType
        {
            reloadTime = 0,
            health = 1,
            armor = 2,
            damage = 3,
            stamina = 4

        }
        
        public void CleanData()
        {
            OnPanelClosed -= Deactivate;
            OnPanelOpen -= ActivatePanel;
        }

        public void OnSpawn()
        {
        }

    }
}