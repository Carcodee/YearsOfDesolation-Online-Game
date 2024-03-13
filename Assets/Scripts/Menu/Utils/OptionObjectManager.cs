using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionObjectManager : MonoBehaviour
{

    public TextMeshProUGUI number;
    public Scrollbar scrollbar;

    public TextMeshProUGUI title;

    public void SyncronizeTextAndSlider()
    {
        number.text = scrollbar.value.ToString("0.00");
    }
    public void SetValueFromScrollbar(ref float value, float scalefactor= 1.0f)
    {
        value = scrollbar.value*scalefactor;
    }
    
    
}
