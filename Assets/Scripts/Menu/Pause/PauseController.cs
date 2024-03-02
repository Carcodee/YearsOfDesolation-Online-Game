using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject optionsMenu;
    public GameObject helpMenu;
    void Start()
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
    }
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
    }
    public void OpenOptions()
    {
        optionsMenu.SetActive(true);
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
