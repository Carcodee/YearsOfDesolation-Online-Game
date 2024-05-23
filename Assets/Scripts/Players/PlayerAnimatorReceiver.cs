using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorReceiver : MonoBehaviour
{
    public PlayerSoundController playerSoundController;

    public void PlayFootStep(float volume)
    {

        playerSoundController.footSteepAudioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        playerSoundController.footSteepAudioSource.volume = UnityEngine.Random.Range( volume/2, volume);
        playerSoundController.footSteepAudioSource.PlayOneShot(playerSoundController.currentStepSound);
        
    }
    

}

