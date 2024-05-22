using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorReceiver : MonoBehaviour
{
    public PlayerSoundController playerSoundController;

    public void PlayFootStep(float volume)
    {
        playerSoundController.footSteepAudioSource.Stop();
        playerSoundController.footSteepAudioSource.volume = volume;
        playerSoundController.footSteepAudioSource.PlayOneShot(playerSoundController.currentStepSound);
        
    }
    

}

