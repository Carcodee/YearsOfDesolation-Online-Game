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
using UnityEngine.Serialization;
using ProgressBar = Michsky.UI.ModernUIPack.ProgressBar;
using Random = UnityEngine.Random;

public class CanvasController : MonoBehaviour
{
    Canvas canvas;
    public RectTransform canvasRect;
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
    public TextMeshProUGUI healthValue;
    public Material hpUIMat;
    public Material followUIHpMat;
    private bool isFollowing;
    private float followValue;
    private float followValTemp;
    private float followTime;
    private float targetVal; 
    private int stackPointer=1;
    private int maxStackPointer=1;
    public HealthType[] healths;
    
    public Stack<HealthType> healthsStack=new Stack<HealthType>();
    public HealthType greenHealth=new HealthType();
    public HealthType backgroundHealth=new HealthType();
    public bool isOnHpBar=true;
    

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
    
    
    [Header("Notifications")]
    public TextMeshProUGUI ammoAddedText;
    public TextMeshProUGUI moneyAddedText;

    [Header("CoinUI")]
    public GameObject coinUI;
    public float closeDistance=10;
    public float largeDistance=40;
    public CanvasGroup coinImageGroup;
    
    void Start()
    {
    
        GetComponents();
        timeToSpawnHolder = GameController.instance.respawnTime;
        timeToSpawnTimer = GameController.instance.respawnTime;
        playerAssigned.health.OnValueChanged += SetStats;
        OnUpdateUI += SetUIElements;
        OnBulletsAddedUI += TotalBulletsAnimation;
        playerAssigned.avaliblePoints.OnValueChanged+=AddMoneyAnimation;
        //TODO: bullets are not being updated
        currentBullets.text = playerAssigned.currentWeaponSelected.ammoBehaviour.currentBullets.ToString() ;
        secondWeaponBullets.text = playerAssigned.onBagWeapon.ammoBehaviour.currentBullets.ToString();
        currentWeaponImage.sprite = playerAssigned.onBagWeapon.weapon.weaponImage;
        secondWeaponImage.sprite = playerAssigned.onBagWeapon.weapon.weaponImage;
        healthsStack.Push(backgroundHealth);
        healthsStack.Push(greenHealth);
        canvasRect = canvas.GetComponent<RectTransform>();
        coinImageGroup = coinUI.GetComponent<CanvasGroup>();
    }



    public void TrackCoin()
    {
        if (playerAssigned.coinPosition==null)
        {
            return;
        }

        Vector3 coinPos = playerAssigned.transform.position;
        
        Vector3 coinInScreen=playerController.cam.WorldToScreenPoint(playerAssigned.coinPosition.transform.position);
        Vector3 magYPos=playerController.cam.WorldToScreenPoint(playerAssigned.coinPosition.magPosition.transform.position);
        
        coinInScreen.x = coinInScreen.x - canvasRect.sizeDelta.x/2;
        magYPos.y = magYPos.y - canvasRect.sizeDelta.y/2;
        
        coinUI.transform.localPosition = new Vector3(coinInScreen.x,magYPos.y,0);
 
        // set it



    }

    public void CheckAlpha()
    {
        if (!coinUI.activeSelf || playerAssigned.coinPosition == null)return;
        
        float distanceToCoin = Vector3.Distance(playerAssigned.transform.position, playerAssigned.coinPosition.transform.position);
        float alpha = Mathf.InverseLerp(closeDistance,largeDistance, distanceToCoin);
        coinImageGroup.alpha = alpha;
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
        TrackCoin();
        CheckAlpha();
    
    }
    
    public void SetStats(float oldValue, float newValue)
    {
        SetUIElements();
    }

    public void SetUIElements()
    {

        if (playerAssigned.GetMaxHealth()==0||playerAssigned.GetMaxHealth()==null)
        {
            return;
        }


        int newStackPointer= (int)Math.Ceiling(playerAssigned.GetHealth()/playerAssigned.startGameHealth);
        
        targetVal = (playerAssigned.GetHealth()-((newStackPointer-1)*10)) / playerAssigned.startGameHealth;
        //
        if (playerAssigned.GetHealth()>playerAssigned.startGameHealth)
        {
            if (newStackPointer>stackPointer)
            {
                stackPointer = newStackPointer;
                if (stackPointer>maxStackPointer)
                {
                    maxStackPointer = stackPointer;
                    PushBar(GetRandomBar());    
                }
                
            }else if (newStackPointer<stackPointer)
            {
                stackPointer = newStackPointer;
            }
            SetBarsAtPointerLocation(hpUIMat);
        }
        if (playerAssigned.GetHealth()<=playerAssigned.startGameHealth)
        {
            targetVal = playerAssigned.GetHealth() / playerAssigned.startGameHealth;
            SetBarDefaultValues(hpUIMat);
        }

        hpUIMat.SetFloat("_HP",targetVal);
        isFollowing = true;
        followValTemp = followValue;
        healthValue.text = "%" + ((playerAssigned.GetHealth() / playerAssigned.startGameHealth)*100).ToString();
        //
    }

    private HealthType GetRandomBar()
    {
        HealthType newHealthType = new HealthType();
                


        newHealthType.backgroundHealthColor = new Color(Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f),1.0f);
        newHealthType.healthColor =new Vector4(newHealthType.backgroundHealthColor.r,newHealthType.backgroundHealthColor.g,newHealthType.backgroundHealthColor.b,5.0f);

        newHealthType.texture= greenHealth.texture;
        return newHealthType;
        
    }
    private void SetBarsAtPointerLocation(Material material)
    {
        if (stackPointer>=2)
        {
            HealthType[] stackArray = healthsStack.ToArray();
            healths = healthsStack.ToArray();

            int top = (healthsStack.Count-1)-stackPointer;
            int secondTop = top+1;
            material.SetColor("_BackgroundCol",stackArray[secondTop].healthColor);
            material.SetTexture("_BackGroundText",stackArray[secondTop].texture);
            
            material.SetColor("_CurrentBarColor",stackArray[top].backgroundHealthColor);
            material.SetTexture("_CurrentBarTexture",stackArray[top].texture);
            Debug.Log("Top: " + top);
            Debug.Log("Second Top: " + secondTop);
        }
    }
    private void SetBarDefaultValues(Material material)
    {
        HealthType[] stackArray = healthsStack.ToArray();

        healths = healthsStack.ToArray();
        material.SetColor("_BackgroundCol",stackArray[healthsStack.Count-1].healthColor);
        material.SetTexture("_BackGroundText",stackArray[healthsStack.Count-1].texture);
        material.SetColor("_CurrentBarColor",stackArray[healthsStack.Count-2].healthColor);
        material.SetTexture("_CurrentBarTexture",stackArray[healthsStack.Count-2].texture);
    }
    public void PushBar(HealthType newHealthType)
    {
        healthsStack.Push(newHealthType);
    }

    public void PopBar(Material material)
    {
        if (healthsStack.Count<=2)
        {
            Debug.Log("Can't pop more health bars");
            return;
        }
        healthsStack.Pop();
        material.SetColor("_CurrentBarColor",healthsStack.Peek().healthColor);
        material.SetTexture("_MainTex",healthsStack.Peek().texture);

        HealthType healthBeforeTheTop = healthsStack.ToArray()[healthsStack.Count-2];

        material.SetColor("_BackGroundCol",healthBeforeTheTop.healthColor);
        material.SetTexture("_BackGroundText",healthBeforeTheTop.texture);
        
    }

    public void SetDefaultVaues(Material material)
    {
        material.SetColor("_BackGroundCol",backgroundHealth.healthColor);
        material.SetTexture("_BackGroundText",backgroundHealth.texture);

        material.SetColor("_CurrentBarColor",greenHealth.healthColor);
        material.SetTexture("_MainTex",greenHealth.texture);
        material.SetFloat("_HP", 1);
    }
    public void FollowHPBar()
    {
        if (!isFollowing)return;
        followTime += Time.deltaTime;
        followValue = Mathf.Lerp(followValTemp, targetVal, followTime);
        followUIHpMat.SetFloat("_HP",followValue);

        if (followTime>=1)
        {
            followTime = 0;
            isFollowing = false;

            followValue = targetVal;
            followUIHpMat.SetFloat("_HP",followValue);

        }

    }
    private void OnApplicationQuit()
    {
        SetDefaultVaues(hpUIMat);
        
        // followUIHpMat.SetFloat("_HP",1);

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
        int moneyAdded = newVal - oldVal;

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
    [System.Serializable]
    public struct HealthType
    {
        [ColorUsageAttribute(true, true)]
        public Color healthColor;
        
        public Color backgroundHealthColor;

        public Texture2D texture;
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