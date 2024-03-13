using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Michsky.UI.Heat;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Dropdown = Michsky.UI.Heat.Dropdown;

public class PauseController : MonoBehaviour
{
    
    
    
    [Header("Main Menu")]
    public GameObject pauseMenu;
    
    [Header("Options")]

    public Animator optionsAnimator;
    public GameObject optionsMenu;
    public CrosshairCreator crosshairCreator;
    
    public OptionObjectManager sensitivity;
    public OptionObjectManager volume;
    public OptionObjectManager brightness;
    
    
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


    public void ConfirmChanges()
    {
        playerCrosshair.crossHair.CopyCrosshair(crosshairCreator.crossHair);
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
        
        
        Debug.Log("Crosshair: " + crosshairCreator.crossHair.width + " " + crosshairCreator.crossHair.length + " " + crosshairCreator.crossHair.gap);
        
        
        // crosshairCreator.crossHair.color = ;
    }
    

    public void SetBrightness()
    {
    }
    public void SetVolume()
    {
    }
    public void SetSensitivity()
    {
    }
    

    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !pauseMenu.activeSelf)
        {
            PauseGame();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeSelf)
        {
            ResumeGame();
        }    
    }
    public void ExitGame()
    {
        Application.Quit();
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

    
}
