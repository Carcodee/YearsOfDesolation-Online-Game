using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Michsky.UI.Heat;
using Third_Party.Modern_UI_Pack.Scripts.Slider;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEditor;
using ProgressBar = Michsky.UI.ModernUIPack.ProgressBar;

public class CanvasController : MonoBehaviour
{
    Canvas canvas;
    
    [Header("Game")]
    public TextMeshProUGUI timeLeft;
    public TextMeshProUGUI playersAlive;
    public TextMeshProUGUI playersConnected;
    public static Action OnUpdateUI;
    public static Action OnBulletsAddedUI;
    public static Action OnMoneyAddedUI;
    
    public Michsky.UI.Heat.ProgressBar timeLeftBar;
    public TextMeshProUGUI timeLeftBarText;
    
    [Header("Player")]
    public TextMeshProUGUI level;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI exp;

    [Header("HUD")]
    public TextMeshProUGUI currentBullets;
    public QuestItem weaponName;
    public RadialSlider expSlider;
    public TextMeshProUGUI totalAmmo;
    public TextMeshProUGUI TotalOnBagAmmo;
    public Image currentWeaponImage;
    public Image secondWeaponImage;
    public TextMeshProUGUI secondWeaponBullets;
    
    
    [Header("DeadScreen")] 
    public TextMeshProUGUI timeToSpawn;
    float timeToSpawnTimer;
    float timeToSpawnHolder;

    [Header("Buttons")]
    public Button openStatsButton;
    
    [Header("Health")]
    public ProgressBar healthBar;

    [Header("Reloading")]
    public SliderManager sliderManager;
    public GameObject reloadingObject;
    
    [Header("Ref")]
    public PlayerStatsController playerAssigned;
    public PlayerController playerController;
    
    [Header("Animation")]
    public GameObject stagesObject;
    public TextMeshProUGUI stagesText;
    public Animator stagesAnimator;
    public bool stageObjectPlaying=false;
    bool BattleRoyaleShowed = false;
    bool FarmShowed = false;
    public Animator[] totalHudAnimators;
    public Animator moneyAnimator;
    
    
    [Header("Notifications")]
    public TextMeshProUGUI ammoAddedText;
    public TextMeshProUGUI moneyAddedText;
    public TextMeshProUGUI currentMoney;

    
    
    void Start()
    {
    
        GetComponents();
        timeToSpawnHolder = GameController.instance.respawnTime;
        timeToSpawnTimer = GameController.instance.respawnTime;
        playerAssigned.health.OnValueChanged += SetStats;
        OnUpdateUI += SetStats;
        OnBulletsAddedUI += TotalBulletsAnimation;
        playerAssigned.avaliblePoints.OnValueChanged+=AddMoneyAnimation;
        //TODO: bullets are not being updated
        currentBullets.text = playerAssigned.currentWeaponSelected.ammoBehaviour.currentBullets.ToString() ;
        secondWeaponBullets.text = playerAssigned.onBagWeapon.ammoBehaviour.currentBullets.ToString();
        currentWeaponImage.sprite = playerAssigned.onBagWeapon.weapon.weaponImage;
        secondWeaponImage.sprite = playerAssigned.onBagWeapon.weapon.weaponImage;
        
        currentMoney.text = (playerAssigned.GetAvaliblePoints()*10).ToString()+ "$";


    }

  

    void Update()
    {
        stageObjectPlaying = stagesObject.activeSelf;
        SetTimer();
        DisplayPlayersConnected();
        DisplayBullets();
        Reloading();
        if (playerController.stateMachineController.currentState.stateName== "Dead")
        {
            timeToSpawn.gameObject.SetActive(true);
            timeToSpawnTimer -= Time.deltaTime;
            timeToSpawn.text = "Time to respawn: " + (timeToSpawnTimer).ToString("0.0");
        }
        else
        {
            timeToSpawnTimer = timeToSpawnHolder;
            timeToSpawn.gameObject.SetActive(false);
        }
    
    }

    public void Reloading()
    {
        if (playerAssigned.currentWeaponSelected.ammoBehaviour.isReloading)
        {
            reloadingObject.SetActive(true);
            sliderManager.mainSlider.maxValue = playerAssigned.currentWeaponSelected.ammoBehaviour.reloadTime.statValue;
            sliderManager.mainSlider.value = playerAssigned.currentWeaponSelected.ammoBehaviour.reloadTime.statValue - playerAssigned.currentWeaponSelected.ammoBehaviour.reloadCurrentTime;
            sliderManager.UpdateUI();
        }
        else
        {
            reloadingObject.SetActive(false);
        }
        
        
    }

    public void AddMoneyAnimation(int oldVal, int newVal)
    {
        currentMoney.text = (playerAssigned.GetAvaliblePoints()*10).ToString()+ "$";
        int moneyAdded = newVal - oldVal;
        moneyAnimator.Play("MoneyAddedOnPanel");

        if (moneyAdded>0)
        {
            moneyAddedText.gameObject.SetActive(true);
            moneyAddedText.text = "+ $" + (moneyAdded*10).ToString();
            moneyAddedText.color=Color.green;
        }
        if (moneyAdded<0)
        {
            moneyAddedText.gameObject.SetActive(true);
            moneyAddedText.text = "- $" + (moneyAdded*10).ToString();
            moneyAddedText.color=Color.red;
        }

    }
    public void TotalBulletsAnimation()
    {
        for (int i = 0; i < totalHudAnimators.Length; i++)
        {
            totalHudAnimators[i].Play("BulletsAdded");
        }

        BulletsOnScreenPopup();
    }

    public void BulletsOnScreenPopup()
    {
        ammoAddedText.gameObject.SetActive(true);
    }
    public void SetStats(float oldValue, float newValue)
    {
        /*(float) playerAssigned.maxHealth*10*/
        healthBar.currentPercent =  newValue *10;
        // while(healthBar.currentPercent>playerAssigned.GetHealth()*10)
        // {
        //     healthBar.currentPercent-=1;
        //     if (healthBar.currentPercent<= playerAssigned.GetHealth()*10)
        //     {
        //         healthBar.currentPercent = playerAssigned.GetHealth()*10;
        //     }
        // }
        
        Debug.Log("Health: " + playerAssigned.GetHealth());
    }
    public void SetStats()
    {
        /*(float) playerAssigned.maxHealth*10*/
        healthBar.currentPercent = playerController.playerStats.GetHealth()*10;
        // while(healthBar.currentPercent>playerAssigned.GetHealth()*10)
        // {
        //     healthBar.currentPercent-=1;
        //     if (healthBar.currentPercent<= playerAssigned.GetHealth()*10)
        //     {
        //         healthBar.currentPercent = playerAssigned.GetHealth()*10;
        //     }
        // }
        
        Debug.Log("Health: " + playerAssigned.GetHealth());
    }
    private void GetComponents()
    {
        canvas = GetComponent<Canvas>();
        playerAssigned = GetComponentInParent<PlayerStatsController>();
        playerController = GetComponentInParent<PlayerController>();
    }
    private void SetTimer()
    {
        //waiting time
        if (!GameController.instance.started.Value)
        {
            timeLeft.text = "Time to start: " + (GameController.instance.waitingTime - GameController.instance.netTimeToStart.Value).ToString("0.0");
            timeLeftBarText.text = "Current Stage: Waiting";
            timeLeftBar.maxValue = GameController.instance.waitingTime;
            timeLeftBar.currentValue = GameController.instance.waitingTime - GameController.instance.netTimeToStart.Value;
            timeLeftBar.UpdateUI();

        }
        //farm time
        if (GameController.instance.started.Value&& !GameController.instance.mapLogic.Value.isBattleRoyale)
        {
            if (!FarmShowed)
            {
                DoOnce.DoOnceMethod(()=> PlayStageAnimation("FARM STAGE"));
                FarmShowed = true;                
            }

            float temp =  GameController.instance.mapLogic.Value.totalTime - GameController.instance.farmStageTimer;
            timeLeft.text = "Farm time: " + temp.ToString("0.0");

            timeLeftBarText.text = "Current Stage: Farm";
            timeLeftBar.maxValue = GameController.instance.mapLogic.Value.totalTime;
            timeLeftBar.currentValue = temp;
            timeLeftBar.UpdateUI();

        }
        //battle royale time
        else if(GameController.instance.started.Value && GameController.instance.mapLogic.Value.isBattleRoyale)
        {
            if (!BattleRoyaleShowed)
            {
                DoOnce.DoOnceMethod( ()=> PlayStageAnimation("BATTLE ROYALE"));

                BattleRoyaleShowed = true;                
            }

            timeLeft.text = "Battle Royale stage";
            timeLeftBarText.text = "Current Stage: Battle Royale";
            timeLeftBar.gameObject.SetActive(false);

        }

    }
    private void DisplayPlayersConnected()
    {
        if (!GameController.instance.started.Value)
        {
            playersConnected.text = "Players Connected: " + GameController.instance.numberOfPlayers.Value.ToString();
        }
        else
        {
            playersConnected.text = "Players Alive: " + GameController.instance.numberOfPlayersAlive.Value.ToString();
        }
    }
    public void DisplayBullets()
    {
        currentBullets.text = playerAssigned.currentWeaponSelected.ammoBehaviour.currentBullets.ToString() ;
        totalAmmo.text = playerAssigned.currentWeaponSelected.ammoBehaviour.totalAmmo.ToString();
        // currentBullets.text = playerAssigned.currentBullets.ToString();
        currentWeaponImage.sprite = playerAssigned.currentWeaponSelected.weapon.weaponImage;
        
        secondWeaponBullets.text = playerAssigned.onBagWeapon.ammoBehaviour.currentBullets.ToString();
        secondWeaponImage.sprite = playerAssigned.onBagWeapon.weapon.weaponImage;
        TotalOnBagAmmo.text = playerAssigned.onBagWeapon.ammoBehaviour.totalAmmo.ToString();


    }
    private void DisplayHP()
    {


    }
    private void DisplayLevel()
    {

        level.text ="Current Level: " +playerAssigned.GetLevel().ToString();
    }

    public void PlayStageAnimation(string stageText)
    {
        stagesObject.SetActive(true);
        stagesText.text = stageText;
    }
   
}

public static class DoOnce
{
    private static bool isDone;
    private static Action myAction;
    
    
    public static void DoOnceMethod(Action action)
    {
        if (myAction!= action)
        {
            isDone = false;
        }
        if (!isDone)
        {
            myAction = action;
            action?.Invoke();
            isDone = true;
        }
    }

}