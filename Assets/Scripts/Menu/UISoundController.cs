using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISoundController : MonoBehaviour
{
    public void PlayHoverUI()
    {
        AudioManager.instance.PlayHoverSound();
    }
    public void PlayClickUI()
    {
        AudioManager.instance.PlayClickSound();
    }
    public void PlayEventSuccedUI()
    {
        AudioManager.instance.EventSucceded();
    }
    public void PlayEventCancelledUI()
    {
        AudioManager.instance.PlayBuyNotAllowed();
    }
    public void PlayOpenShopUI()
    {
        AudioManager.instance.OpenShopSound();
    }   
    public void PlayOpenPause()
    {
        AudioManager.instance.OpenPauseSound();
    }
}
