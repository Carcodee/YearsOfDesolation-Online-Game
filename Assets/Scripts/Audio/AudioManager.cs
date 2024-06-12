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
    public AudioClip newStageSound;
    public AudioClip displayTutorialDataSound;
    public AudioClip winSound;
    
    [Header("Player")]
    public AudioClip eventSuccedSound;
    public AudioClip eventCanceledSound;
    public AudioClip openShopSound;
    public AudioClip openPauseSound;
    public AudioClip dangerSound;

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

    public void PlayWinSound()
    {
        UIAudioSource.PlayOneShot(winSound, 0.1f);
    }
    public void DangerSound()
    {
        UIAudioSource.PlayOneShot(dangerSound);
    }

    public void OpenShopSound()
    {
        UIAudioSource.PlayOneShot(openShopSound);
    }

    public void OpenPauseSound()
    {
        UIAudioSource.PlayOneShot(openPauseSound);
    }

    public void PlayNewStage()
    {
        UIAudioSource.PlayOneShot(newStageSound);
    }
    public void PlayNewStepSound()
    {
        UIAudioSource.PlayOneShot(displayTutorialDataSound);
    }
    
    public void SetBackgroundSound(float val)
    {
        mixer.SetFloat("EnvironmentVolume",Mathf.Log(val) *20);
    }
    public void SetGameplaySound(float val)
    {
        mixer.SetFloat("PlayerVolume",Mathf.Log(val) * 20);
    }
    public void SetMasterSound(float val)
    {
        mixer.SetFloat("MasterVolume",Mathf.Log(val) * 20);
    }
    
}
