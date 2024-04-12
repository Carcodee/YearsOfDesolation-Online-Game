using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationItem : MonoBehaviour
{
    public static Action<string> OnTextDisplay;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI textToDisplay;
    
    private void Start()
    {

        OnTextDisplay += DisplayText;
    }

    void DisplayText(string text)
    {
        textToDisplay.text = text;
    }
}
