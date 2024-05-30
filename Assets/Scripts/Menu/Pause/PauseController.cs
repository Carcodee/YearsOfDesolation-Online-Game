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
    
    [Header("Options")]

    public Animator optionsAnimator;
    public GameObject optionsMenu;
    public CrosshairCreator crosshairCreator;
    
    public OptionObjectManager sensitivity;
    public OptionObjectManager playerVolume;
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
    public GameObject helpMenu;
    //Options
    
    [Header("Ref")]
    public PlayerController playerAssigned;
    public CrosshairCreator playerCrosshair;
    public GameObject SkillPanel;

    [Header("Ref")]
    public GameObject alertPanel;

    public TextMeshProUGUI alertText;
    
    public AudioMixer mixer;
    // Update is called once per frame
    void Update()
    {
        if (alertPanel.activeSelf)
        {
            alertText.text = $"Come back Soldier!\n (TIME TO DIE: {(playerAssigned.playerStats.outsideOfMapTickTime-playerAssigned.playerStats.currentOutsideOfMapTimer).ToString("0.0")})";
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

    private void Start()
    {
        
        INetObjectToClean objectToClean = GetComponent<INetObjectToClean>();
        CleanerController.instance.AddObjectToList(objectToClean);

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
    }
    public void SetGameplaySound()
    {
        mixer.SetFloat("PlayerVolume",Mathf.Log(playerVolume.slider.mainSlider.value) * 20);
    }
    public void SetMasterSound()
    {
        mixer.SetFloat("MasterVolume",Mathf.Log(masterVolume.slider.mainSlider.value) * 20);
    }
    public void SetSensitivity()
    {
        playerAssigned.mouseSensitivity = sensitivity.slider.mainSlider.value;
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
        PostProccesingManager.instance.ActivateBlur(1);
    }
    public void ResumeGame()
    {
        optionsMenu.SetActive(false);
        helpMenu.SetActive(false);
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
    public void OpenControls(bool value)
    {
        if (!helpMenu.activeSelf)
        {
            helpMenu.SetActive(true);
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
