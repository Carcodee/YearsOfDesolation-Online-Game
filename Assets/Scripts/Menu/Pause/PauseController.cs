using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Menu.StatsPanel;
using Michsky.UI.Heat;
using NetworkingHandling;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Dropdown = Michsky.UI.Heat.Dropdown;

public class PauseController : MonoBehaviour, INetObjectToClean
{

    public static Action<bool> OnAlertActivated;
    [Header("Main Menu")]
    public GameObject pauseMenu;

    public GameObject controlsPanel;
    
    [Header("Options")]

    public Animator optionsAnimator;
    public GameObject optionsMenu;
    public CrosshairCreator crosshairCreator;
    
    public OptionObjectManager sensitivity;
    public OptionObjectManager gameplayVolume;
    public OptionObjectManager backGroundVolume;
    public OptionObjectManager masterVolume;
    
    [Header("Croshair")]
    //Crosshair
    public Dropdown colorDropdown;
    public Toggle staticToggle;
    
    public Scrollbar crosshairThickness;
    public Scrollbar crosshairLength;
    public Scrollbar crosshairGap;

    public Color crosshairColor;

    public TextMeshProUGUI lengthText;
    public TextMeshProUGUI thicknessText;
    public TextMeshProUGUI gapText;
    
    
    [Header("Controls")]
    public Animator controlsAnimator;
    //Options
    
    [Header("Ref")]
    public PlayerController playerAssigned;
    public CrosshairCreator playerCrosshair;
    public GameObject SkillPanel;

    [Header("Ref")]
    public GameObject alertPanel;

    public TextMeshProUGUI alertText;
    
    public AudioMixer mixer;

    public GameObject openBuildSpamer; 
    public GameObject startGameLayout; 
    public TextMeshProUGUI startGameText;

    private string startGameTextBuffer;
    // Update is called once per frame
    void Update()
    {
        if (alertPanel.activeSelf)
        {
            alertText.text = $"Come back Soldier!\n (TIME TO DIE: {(playerAssigned.playerStats.outsideOfMapTickTime-playerAssigned.playerStats.currentOutsideOfMapTimer).ToString("0.0")})";
        }


        StartGamePanel();

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (openBuildSpamer.gameObject.activeSelf)
            {
                openBuildSpamer.gameObject.SetActive(false);
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape) && !pauseMenu.activeSelf)
        {
            PlayerComponentsHandler.IsCurrentDeviceMouse = true;
            PauseGame();
            AudioManager.instance.OpenPauseSound();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeSelf)
        {

            PlayerComponentsHandler.IsCurrentDeviceMouse = false;
            ResumeGame();
            AudioManager.instance.OpenPauseSound();
            if (!GameManager.Instance.isOnTutorial)return;
            if (!TutorialStagesHandler.instance.currentStage.hasDialogFinished)PlayerComponentsHandler.IsCurrentDeviceMouse = true;

        }

        if (Input.GetKeyDown(KeyCode.B)&& !SkillPanel.activeSelf)
        {
            
            if (GameManager.Instance.isOnTutorial && !TutorialStagesHandler.instance.stagesDone.Any(x=> x.stage==TutorialStage.PlayerZone))
            { 
                return;
            }
            
            AudioManager.instance.OpenShopSound();
            SkillPanel.SetActive(true);
            StatsPanelController.OnPanelOpen.Invoke();
        }

    }

    public void StartGamePanel()
    {
        if (playerAssigned.IsSpawned &&playerAssigned.IsServer && !GameManager.Instance.isOnTutorial)
        {
            if (GameController.instance.started.Value)
            {
                if (startGameLayout.gameObject.activeSelf)
                {
                    startGameLayout.gameObject.SetActive(false);
                }
                return;
            }
            if (!GameController.instance.started.Value && GameController.instance.numberOfPlayers.Value <= 2 )
            {
                startGameLayout.gameObject.SetActive(true);
                startGameText.text = "Waiting for <color=red>1</color> more player...";
            }
            if (!GameController.instance.started.Value && GameController.instance.numberOfPlayers.Value >= 2)
            {
                startGameLayout.gameObject.SetActive(true);
                startGameText.text = startGameTextBuffer;
                 
            }           
        }
    }
    private void Start()
    {
        
        INetObjectToClean objectToClean = GetComponent<INetObjectToClean>();
        CleanerController.instance.AddObjectToList(objectToClean);
        if (!GameManager.Instance.isOnTutorial)
        {
            StartCoroutine(SpawnGameObjectAfterSeconds(2.0f, openBuildSpamer));
        }

        startGameTextBuffer = startGameText.text;


    }

    public IEnumerator SpawnGameObjectAfterSeconds(float time, GameObject gameObject)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(true);
    }

    public void ConfirmChanges()
    {
        playerCrosshair.crossHair.CopyCrosshair(crosshairCreator.crossHair);
    }

    private void OnEnable()
    {
        OnAlertActivated += ActivateAlert;
    }

    public void ActivateAlert(bool val)
    {
        alertPanel.SetActive(val);
    }
    public void TogleStatic()
    {
        crosshairCreator.crossHair.isStatic = !staticToggle.isOn;

    }

    public void LoadOptions()
    {
        
    }
    
    public void OpenControls(bool val)
    {
        controlsPanel.gameObject.SetActive(val);
    }
    public void SetCrosshair()
    {
        crosshairCreator.crossHair.width =math.clamp(crosshairThickness.value*50, 1, crosshairThickness.value*50);
        crosshairCreator.crossHair.length =math.clamp(crosshairLength.value*50, 1, crosshairLength.value*50);
        crosshairCreator.crossHair.gapBuffer =math.clamp(crosshairGap.value*50, 1, crosshairGap.value*50);
        thicknessText.text = crosshairThickness.value.ToString("0.00");
        lengthText.text = crosshairLength.value.ToString("0.00");
        gapText.text = crosshairGap.value.ToString("0.00");
        
        
    }
    

    public void SetBackgroundSound()
    {
        mixer.SetFloat("EnvironmentVolume",Mathf.Log(backGroundVolume.slider.mainSlider.value) *20);
        GameManager.Instance.backgroundSoundBeforeStart = backGroundVolume.slider.mainSlider.value;
    }
    public void SetGameplaySound()
    {
        mixer.SetFloat("PlayerVolume",Mathf.Log(gameplayVolume.slider.mainSlider.value) * 20);
        GameManager.Instance.gameplaySoundBeforeStart = gameplayVolume.slider.mainSlider.value;
    }
    public void SetMasterSound()
    {
        mixer.SetFloat("MasterVolume",Mathf.Log(masterVolume.slider.mainSlider.value) * 20);
        GameManager.Instance.masterSoundBeforeStart = masterVolume.slider.mainSlider.value;
    }
    public void SetSensitivity()
    {
        playerAssigned.mouseSensitivity = sensitivity.slider.mainSlider.value;
    }
    

    public void GetBackgroundSound()
    {
        backGroundVolume.slider.mainSlider.value = GameManager.Instance.backgroundSoundBeforeStart;
    }
    public void GetGameplaySound()
    {
        gameplayVolume.slider.mainSlider.value =  GameManager.Instance.gameplaySoundBeforeStart;
    }
    public void GetMasterSound()
    {
        masterVolume.slider.mainSlider.value = GameManager.Instance.masterSoundBeforeStart;
    }
    public void GetSensitivity()
    {
        sensitivity.slider.mainSlider.value = playerAssigned.mouseSensitivity;
    }
    
    
    public void ExitGame()
    {
        GameManager.Instance.gameEnded = true;
        PostProccesingManager.instance.ActivateBlur(0.0f);
        PostProccesingManager.instance.ActivateMenuBlur(1.0f);
        if (GameManager.Instance.localPlayerRef.IsServer)
        {
            NetworkingHandling.HostManager.instance.DisconnectHost();
        }
        else
        {
            ClientManager.instance.DisconnectClient(GameManager.Instance.localPlayerRef.OwnerClientId);
        }
    }
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        GetSensitivity();
        GetMasterSound();
        GetGameplaySound();
        GetBackgroundSound();
        PostProccesingManager.instance.ActivateBlur(1);
    }
    public void ResumeGame()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        PostProccesingManager.instance.ActivateBlur(0);

    }
    public void OpenOptions(bool value)
    {
        if (!optionsMenu.activeSelf)
        {
            optionsMenu.SetActive(true);
        }
        optionsAnimator.SetBool("FadeIn", value);
    }


    public void CleanData()
    {
        OnAlertActivated -= ActivateAlert;
    }

    public void OnSpawn()
    {
    }

    public bool shutingDown { get; set; }
}
