using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorReceiver : MonoBehaviour
{
    public PlayerSoundController playerSoundController;

    public void PlayFootStep(int feetNumber)
    {
        float volumeToEmit = 0.03f;
        if (playerSoundController.IsOwner)
        {
            // volumeToEmit = UnityEngine.Random.Range(volume/1.5f, volume);
        }
        else
        {
            volumeToEmit = UnityEngine.Random.Range( 0.1f, 0.5f);
        }
        
        playerSoundController.footSteepAudioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        playerSoundController.footSteepAudioSource.volume = volumeToEmit;
        playerSoundController.footSteepAudioSource.PlayOneShot(playerSoundController.currentStepSound);
        if (feetNumber==-1)
        {
            return;
        }
        playerSoundController.playerRef.playerStats.playerVFXController.SpawnStepVfx(feetNumber);

    }
    

}

