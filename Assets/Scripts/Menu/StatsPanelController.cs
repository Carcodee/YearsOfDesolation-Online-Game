using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatsPanelController : MonoBehaviour
{
    public UnityAction OnPannelOpen;
    public UnityAction OnPannelClosed;

    [Header("References")]
    [SerializeField] private PlayerStatsController playerStatsController;
    [SerializeField]private PlayerVFXController playerVFXController;
    public GameObject [] statsObjects;
    public WindowManager windowManager;
    public Animator panelAnimator;
    [Header("Stats")]
    public TextMeshProUGUI[] statValues;
    private bool isRefreshedStats = false;
    private bool isPanelRefreshed = false;

    [Header("HeadStats")]
    public TextMeshProUGUI level;
    public TextMeshProUGUI avaliblePointsText;

    [Header("Buttons")]
    public ButtonManagerIcon[] addButtons;
    public ButtonManagerIcon[] removeButtons;
    public Button openPannel;

    [Header("Sesion Variables")]

    [SerializeField] private int avaliblePoints;
    [SerializeField] private int sesionPoints;
    public bool isPanelOpen { get;private set;}

    [Header("Animation")]
    public float animationTime;
    public float animationSpeed;
    public float animationFunction => 1 - Mathf.Pow(1 - animationTime, 3);
    public Transform targetPos;
    public Vector3 endPos;
    public Vector3 startPos;

    [Header("Selector")]
    public GameObject selector;
    public GameObject buttonSelector;
    public int selectorIndex=0;
    public int buttonSelectorIndex=1;

    private void OnEnable()
    {
        OnPannelOpen += OpenPanel;
        OnPannelClosed += ClosePanel;
    }

    private void OnDisable()
    {
        OnPannelOpen -= OpenPanel;
        OnPannelClosed -= ClosePanel;
    }

    void Start()
    {
        selectorIndex = 0;
        buttonSelectorIndex = 1;
        isPanelOpen =false;
        playerStatsController = GetComponentInParent<PlayerStatsController>();
        playerVFXController = playerStatsController.GetComponent<PlayerVFXController>();
        endPos= targetPos.position;
        startPos= transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isPanelOpen = !isPanelOpen;
            UpdateStats();

            if (isPanelOpen)
            {
                isPanelRefreshed = false;
            }
            else
            {
                isPanelRefreshed = true;
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

    public void AnimatePanel()
    {
        if (isPanelOpen && animationTime<1)
        {
            animationTime+=Time.deltaTime*animationSpeed;
            Mathf.Clamp(animationTime, 0, 1);
        }
        if (!isPanelOpen&&animationTime>0)
        {
            animationTime-=Time.deltaTime*animationSpeed;
            Mathf.Clamp(animationTime, 0, 1);
        }

        float xPos=Mathf.Lerp(startPos.x, endPos.y, animationFunction);
        transform.position = new Vector3(xPos, transform.position.y, 0);
    }

    void AddListenersToButtons()
    {
        for (int i = 0; i < addButtons.Length; i++)
        {
            addButtons[i].clickEvent.AddListener(() => AddStat(i));
            removeButtons[i].clickEvent.AddListener(() => RemoveStat(i));
        }
    }

    

    public void OpenPanel()
    {
        if (isPanelRefreshed)
        {
            return;
        }
        avaliblePoints = playerStatsController.GetAvaliblePoints();
        avaliblePointsText.text = "Avalible Points: " + avaliblePoints.ToString();
        level.text = "Level: " + playerStatsController.GetLevel().ToString();
        sesionPoints = playerStatsController.GetAvaliblePoints();
        isPanelRefreshed = true;

    }
    public void ClosePanel()
    {
        avaliblePoints = 0;
        sesionPoints = 0;
    }
    public void UpdateStats()
    {
        LoadAllStats();
        avaliblePointsText.text = "Avalible Points: " + avaliblePoints.ToString();
        level.text = "Level: " + playerStatsController.GetLevel().ToString();
    }


    public void LoadAllStats()
    {
        for (int i = 0; i < statValues.Length; i++)
        {
            LoadStat(i);
        }
        isRefreshedStats = true;
        
    }
    public void LoadStat(int statType)
    {
        if (statType> statValues.Length)
        {
            Debug.Log(statType+ "index Stat not found");
            return;            
        }
        
        switch (statType)
        {
            case (int)StatType.reloadTime:
                statValues[statType].text = playerStatsController.GetHaste().ToString();
                break;
            case (int)StatType.health:
                statValues[statType].text = playerStatsController.GetMaxHealth().ToString();
                break;
            case (int)StatType.armor:
                statValues[statType].text = playerStatsController.GetArmor().ToString();
                break;
            case (int)StatType.damage:
                statValues[statType].text = playerStatsController.GetDamageDone().ToString();
                break;
            case (int)StatType.stamina:
                statValues[statType].text = playerStatsController.GetStamina().ToString();
                break;
        }
        
    }
    public void AddStat(int buttonType)
    {

        if (avaliblePoints <= 0)
        {
            Debug.Log("No points");
            return;
        }
        
        switch (buttonType)
        {
            case (int)StatType.reloadTime:
                playerStatsController.SetHasteServerRpc(playerStatsController.GetHaste() + 1);
                playerVFXController.ApplyPointsEffect();
                Debug.Log("AddStat");
                avaliblePoints--;
                break;
            case (int)StatType.health:
                playerStatsController.SetMaxHealthServerRpc(playerStatsController.GetMaxHealth() + 1);
                playerVFXController.ApplyPointsEffect();
                avaliblePoints--;
                break;
            case (int)StatType.armor:
                playerStatsController.SetArmorServerRpc(playerStatsController.GetArmor() + 1);
                playerVFXController.ApplyPointsEffect();
                avaliblePoints--;
                break;
            case (int)StatType.damage:
                playerStatsController.SetDamageServerRpc(playerStatsController.GetDamageDone() + 1);
                playerVFXController.ApplyPointsEffect();
                avaliblePoints--;
                break;
            case (int)StatType.stamina:
                playerStatsController.SetStaminaServerRpc(playerStatsController.GetStamina() + 1);
                playerVFXController.ApplyPointsEffect();
                avaliblePoints--;
                break;
        }
        Debug.Log("Reloaded");
        UpdateStats();
        
    }

    public void RemoveStat(int buttonType)
    {
        switch (buttonType)
        {
            case (int)StatType.reloadTime:
                if (playerStatsController.GetHaste() <= 1)
                {
                    Debug.Log("Cant remove");
                    return;
                }
                playerStatsController.SetHasteServerRpc(playerStatsController.GetHaste() - 1);
                Debug.Log("removed");
                avaliblePoints++;
                break;
            
            case (int)StatType.health:
                if (playerStatsController.GetHealth() <= 1)
                {
                    Debug.Log("Cant remove");
                    return;
                }
                playerStatsController.SetMaxHealthServerRpc(playerStatsController.GetMaxHealth() -1 );

                Debug.Log("removed");
                avaliblePoints++;
                break;
            
            case (int)StatType.armor:
                if (playerStatsController.GetArmor() <= 1)
                {
                    Debug.Log("Cant remove");
                    return;
                }
                playerStatsController.SetArmorServerRpc(playerStatsController.GetArmor() - 1);
                avaliblePoints++;
                break;
            
            case (int)StatType.damage:
                if (playerStatsController.GetDamageDone() <= 1)
                {
                    Debug.Log("Cant remove");
                    return;
                }
                playerStatsController.SetDamageServerRpc(playerStatsController.GetDamageDone() - 1);
                avaliblePoints++;
                break;
            
            case (int)StatType.stamina:
                if (playerStatsController.GetStamina() <= 1)
                {
                    Debug.Log("Cant remove");
                    return;
                }

                playerStatsController.SetStaminaServerRpc(playerStatsController.GetStamina() - 1);
                avaliblePoints++;
                break;
        }
        Debug.Log("Reloaded");
        UpdateStats();
        
    }
    

}

[Serializable]
public enum StatType
{
    reloadTime=0,
    health=1,
    armor=2,
    damage=3,
    stamina=4
        
}