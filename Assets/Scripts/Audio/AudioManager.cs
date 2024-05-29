using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioListener menuListener;
    public AudioSource backGroundAudioSource;
    public AudioSource UIAudioSource;
    public AudioMixer mixer;


    [Header("Menu")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip mainMenuSound;
    public AudioClip inGameSound;
    public AudioClip waitingScreenSound;
    [Header("Player")]
    public AudioClip eventSuccedSound;
    public AudioClip eventCanceledSound;
    public AudioClip openShopSound;
    public AudioClip openPauseSound;

    public void Awake()
    {
        if (instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        backGroundAudioSource.loop = true;
        backGroundAudioSource.clip = mainMenuSound;
        backGroundAudioSource.Play();
        inGameSound.LoadAudioData();
        eventSuccedSound.LoadAudioData();
        eventCanceledSound.LoadAudioData();
        openShopSound.LoadAudioData();
        openPauseSound.LoadAudioData();
    }

    public void PlayGameSound()
    {
        backGroundAudioSource.volume = 0.01f;
        backGroundAudioSource.loop = true;
        backGroundAudioSource.clip = inGameSound;
        backGroundAudioSource.Play();
    }

    public void PlayWaitingScreenSound()
    {
        backGroundAudioSource.volume = 1.0f;
        backGroundAudioSource.loop = true;
        backGroundAudioSource.clip = waitingScreenSound;
        backGroundAudioSource.Play();
    }
    
    public void PlayMenuScreenSound()
    {
        backGroundAudioSource.volume = 0.01f;
        backGroundAudioSource.loop = true;
        backGroundAudioSource.clip = mainMenuSound;
        backGroundAudioSource.Play();
    }
    public void ActivateListener(bool value)
    {
        menuListener.enabled = value;
    }
    
    public void PlayHoverSound()
    {
        UIAudioSource.PlayOneShot(hoverSound);
    }
    public void PlayClickSound()
    {
        UIAudioSource.PlayOneShot(clickSound);
    }

    public void EventSucceded()
    {
        UIAudioSource.PlayOneShot(eventSuccedSound);
    }
    
    public void PlayBuyNotAllowed()
    {
        UIAudioSource.PlayOneShot(eventCanceledSound);
    }

    public void OpenShopSound()
    {
        UIAudioSource.PlayOneShot(openShopSound);
    }

    public void OpenPauseSound()
    {
        UIAudioSource.PlayOneShot(openPauseSound);
    }
    
}
