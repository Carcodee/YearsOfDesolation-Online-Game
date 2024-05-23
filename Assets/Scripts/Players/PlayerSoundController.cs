using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSoundController : NetworkBehaviour
{
    
    public AudioSource movementAudioSource;
    public AudioSource weaponAudioSource;
    public AudioSource footSteepAudioSource;
    public AudioSource actionsAudioSource;
    public AudioListener audioListener;
    public PlayerController playerRef;

    
    
    [Header("Actions")]
    public AudioClip aimOnSound;
    public AudioClip aimOffSound;
    public AudioClip damageTakeSound;
    public AudioClip DeathSound;
    public AudioClip respawningSound;
    public AudioClip playerRespawnedSound;
    public AudioClip hitToEnemySound;
    public AudioClip killDoneSound;
    public AudioClip pickAmmoSound;

    [Header("Weapon")]
    public AudioClip currentWeaponShoot;
    public AudioClip currentStartReload;
    public AudioClip currentEndReload;
    
    
    [Header("Movement")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip slideSound;
    
    [Header("FootSteps")]
    public AudioClip currentStepSound;
    public FootStep[] footSteps;//done
    public Dictionary<GroundType, AudioClip> footStepsList = new Dictionary<GroundType, AudioClip>();//done

    private void Awake()
    {
        for (int i = 0; i < footSteps.Length; i++)
        {
            footSteps[i].Insert(ref footStepsList);
        }
        Init();
        
    }

    public override void OnNetworkSpawn()
    {
        audioListener.enabled = IsOwner;
        
    }

    public void Init()
    {
        if (IsOwner)
        {
            if (playerRef==null)
            {
                playerRef= GetComponent<PlayerController>();
            }

        }

        currentStepSound = footStepsList.GetValueOrDefault(GroundType.metal);
    }

    public void PlaySound(AudioSource audioSource, AudioClip audioClip, float volume = 1.0f, float pitch = 1.0f)
    {
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayCurrentShoot(float volume = 1.0f, float pitch = 1.0f)
    {
        weaponAudioSource.PlayOneShot(currentWeaponShoot);
    }

    public void PlayCurrentStartReload(float volume = 1.0f, float pitch = 1.0f)
    {
        weaponAudioSource.PlayOneShot(currentStartReload);
    }
    
    public void PlayCurrentEndReload(float volume = 1.0f, float pitch = 1.0f)
    {
        weaponAudioSource.PlayOneShot(currentEndReload);
    }
    public void UpdateWeaponSound(WeaponItem weapon)
    {
        currentWeaponShoot = weapon.template.shootSound;
        currentStartReload = weapon.template.reloadStartSound;
        currentEndReload = weapon.template.reloadEndSound;
         
    }


}
[System.Serializable]
public struct FootStep
{
    public GroundType groundType;
    public AudioClip audioClip;
    
    public void Insert(ref Dictionary<GroundType, AudioClip> list)
    {
        if (list==null)
        {
            list = new Dictionary<GroundType, AudioClip>();
        }
        list.Add(groundType, audioClip);
    }
}
public enum GroundType
{
    concrete,
    dirt,
    metal,
    wood
}
