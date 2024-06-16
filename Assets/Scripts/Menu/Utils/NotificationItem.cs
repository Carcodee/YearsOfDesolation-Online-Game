using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.Heat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NotificationItem : MonoBehaviour
{
    public static Action<string> OnTextDisplay;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI textToDisplay;
    public ButtonManager nextButton;
    public ButtonManager backButton;
    public ButtonManager stepDoneButton;
    private void Start()
    {

        OnTextDisplay += DisplayText;
        if (!GameManager.Instance.isOnTutorial)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (TutorialManager.instance==null)return;
        if (TutorialManager.instance.isLastText)
        {
            nextButton.gameObject.SetActive(false); 
            stepDoneButton.gameObject.SetActive(true); 
        }
        else
        {
            nextButton.gameObject.SetActive(true); 
            stepDoneButton.gameObject.SetActive(false);           
        }
        if (TutorialManager.instance.HUBCounter<=0)
        {
            backButton.gameObject.SetActive(false); 
        }
        else
        {
             backButton.gameObject.SetActive(true); 
        }

    }

    void DisplayText(string text)
    { 
        textToDisplay.text = text;
    }
    public void NextHUBButton()
    { 
        TutorialManager.instance.NextHUB(); 
        textToDisplay.text = TutorialManager.instance.tutorialTextData.text;
    }
    public void GoBackHub()
    {
        TutorialManager.instance.GoBackHUB(); 
        textToDisplay.text = TutorialManager.instance.tutorialTextData.text;
    }

    public void StepDoneButton()
    { 
        TutorialManager.instance.StepDone();
        textToDisplay.text = TutorialManager.instance.tutorialTextData.text; 
        stepDoneButton.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

}
