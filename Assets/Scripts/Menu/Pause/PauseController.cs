using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Heat;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    
    
    [Header("Main Menu")]
    public GameObject pauseMenu;
    
    [Header("Options")]
    public GameObject optionsMenu;
    public CrosshairCreator crosshairCreator;
    
    [Header("Controls")]
    public GameObject helpMenu;
    //Options
    public Scrollbar sensitivity;
    public Scrollbar volume;
    public Scrollbar brightness;
    
    
    //Crosshair
    public Scrollbar _crosshairThickness;
    public Scrollbar _crosshairLength;
    public Scrollbar _crosshairGap;
    public Color _crosshairColor;

    
    
    
    public PlayerController playerAssigned;
    
    void Start()
    {
    }

    public void SetStats()
    {
    }
    public void SetCrosshair()
    {
        crosshairCreator.crossHair.width =math.clamp(_crosshairThickness.value*100, 1, _crosshairThickness.value*100);
        crosshairCreator.crossHair.length =math.clamp(_crosshairLength.value*100, 1, _crosshairLength.value*100) ;
        crosshairCreator.crossHair.gap = math.clamp(_crosshairGap.value*100, 1, _crosshairGap.value*100);

        Debug.Log("Crosshair: " + crosshairCreator.crossHair.width + " " + crosshairCreator.crossHair.length + " " + crosshairCreator.crossHair.gap);
        // crosshairCreator.crossHair.color = ;
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
        pauseMenu.SetActive(false);
        PostProccesingManager.instance.ActivateBlur(0);

    }
    public void OpenOptions(bool value)
    {
        optionsMenu.SetActive(value);
    }
    public void OpenHelp()
    {
        helpMenu.SetActive(true);
    }
    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
    }
    
}
