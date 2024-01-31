using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Michsky.UI.ModernUIPack;
using Third_Party.Modern_UI_Pack.Scripts.Slider;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEditor;
public class CanvasController : MonoBehaviour
{
    Canvas canvas;
    [Header("Game")]
    public TextMeshProUGUI timeLeft;
    public TextMeshProUGUI playersAlive;
    public TextMeshProUGUI playersConnected;

    [Header("Player")]
    public TextMeshProUGUI level;
    public TextMeshProUGUI totalAmmo;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI exp;

    public RadialSlider bulletsSlider;
    public RadialSlider expSlider;

    [Header("DeadScreen")] 
    public TextMeshProUGUI timeToSpawn;
    float timeToSpawnTimer;
    float timeToSpawnHolder;

    [Header("Buttons")]
    public Button openStatsButton;
    
    [Header("Health")]
    public ProgressBar healthBar;


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



    void Start()
    {
    
        GetComponents();
        timeToSpawnHolder = GameController.instance.respawnTime;
        timeToSpawnTimer = GameController.instance.respawnTime;
        playerAssigned.health.OnValueChanged += SetStats;
        
        
        //TODO: bullets are not being updated
        bulletsSlider.currentValue = playerAssigned.currentBullets;

    }

  

    void Update()
    {
        stageObjectPlaying = stagesObject.activeSelf;
        SetTimer();
        DisplayPlayersConnected();
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

    public void SetStats(int oldValue, int newValue)
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
    
    private void GetComponents()
    {
        canvas = GetComponent<Canvas>();
        playerAssigned = GetComponentInParent<PlayerStatsController>();
        playerController = GetComponentInParent<PlayerController>();
    }
    private void SetTimer()
    {
        //waiting time
        if (!GameController.instance.started)
        {
            timeLeft.text = "Time to start: " + (GameController.instance.waitingTime - GameController.instance.netTimeToStart.Value).ToString("0.0");
    
        }
        //farm time
        if (GameController.instance.started&& !GameController.instance.mapLogic.Value.isBattleRoyale)
        {
            if (!FarmShowed)
            {
                DoOnce.DoOnceMethod(()=> PlayStageAnimation("FARM STAGE"));
                FarmShowed = true;                
            }

            float temp =  GameController.instance.mapLogic.Value.totalTime - GameController.instance.farmStageTimer;
            timeLeft.text = "Farm time: " + temp.ToString("0.0");
        }
        //battle royale time
        else if(GameController.instance.started && GameController.instance.mapLogic.Value.isBattleRoyale)
        {
            if (!BattleRoyaleShowed)
            {
                DoOnce.DoOnceMethod( ()=> PlayStageAnimation("BATTLE ROYALE"));

                BattleRoyaleShowed = true;                
            }

            timeLeft.text = "Battle Royale stage";
        }

    }
    private void DisplayPlayersConnected()
    {
        if (!GameController.instance.started)
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
        totalAmmo.text = playerAssigned.totalAmmo.ToString();
        bulletsSlider.currentValue = playerAssigned.currentBullets;
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