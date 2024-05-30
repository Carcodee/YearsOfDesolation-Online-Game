using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSoundController : NetworkBehaviour
{

    [Header("AudioSource")]
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
    public AudioClip outsideOffZoneDamage;

    [Header("Weapon")]
    public AudioClip currentWeaponShoot;
    public AudioClip currentStartReload;
    public AudioClip currentEndReload;
    
    public AudioClip weaponChangeStart;
    public AudioClip weaponChangeEnd;
    
    public AudioClip defaultShoot;
    
    [Header("Movement")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip slideSound;
    public AudioClip jetPackSound;
    
    [Header("FootSteps")]
    public AudioClip currentStepSound;
    public FootStep[] footSteps;
    public Dictionary<GroundType, AudioClip> footStepsList = new Dictionary<GroundType, AudioClip>();

    
    
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
        if (playerRef==null)
        {
            playerRef= GetComponent<PlayerController>();
        }
        footSteepAudioSource.spatialBlend = 1.0f;
        weaponAudioSource.spatialBlend = 1.0f;
        actionsAudioSource.spatialBlend = 1.0f;
        movementAudioSource.spatialBlend = 1.0f;
        currentWeaponShoot = defaultShoot;


        currentStepSound = footStepsList.GetValueOrDefault(GroundType.metal);
        
    }

    public void PlaySound(AudioSource audioSource, AudioClip audioClip, float volume = 1.0f, float pitch = 1.0f)
    {
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayWeaponSound(AudioClip audioClip,bool stopLastSound = false, float volume = 1.0f, float pitch = 1.0f)
    {
        if (stopLastSound)weaponAudioSource.Stop();
        weaponAudioSource.volume = volume;
        weaponAudioSource.pitch = pitch;
        weaponAudioSource.PlayOneShot(audioClip);
    }
    public void PlayCurrentShoot(bool stopLast= false, float volume = 1.0f, float pitch = 1.0f)
    {
        if (stopLast)weaponAudioSource.Stop();
        weaponAudioSource.volume = volume;
        weaponAudioSource.pitch = pitch;
        weaponAudioSource.PlayOneShot(currentWeaponShoot);
    }

    public void PlayCurrentStartReload(float volume = 1.0f, float pitch = 1.0f)
    {
        weaponAudioSource.Stop();
        weaponAudioSource.volume = volume;
        weaponAudioSource.pitch = pitch;
        weaponAudioSource.PlayOneShot(currentStartReload);
    }

    public void PlayActionSound(AudioClip audioClip, float volume = 1.0f, float pitch = 1.0f, bool stopBefore= false)
    {
        if (stopBefore)actionsAudioSource.Stop();
        actionsAudioSource.volume = volume;
        actionsAudioSource.pitch = pitch;
        actionsAudioSource.PlayOneShot(audioClip);
    }
    
    public void PlayMovementSound(AudioClip audioClip, bool overrideSound = false ,float volume = 1.0f, float pitch = 1.0f, bool stopBefore= false)
    {
        movementAudioSource.volume = 0.3f;
        if (overrideSound)
        {
           movementAudioSource.volume = volume;
        }
        if (stopBefore)movementAudioSource.Stop();
        movementAudioSource.pitch = pitch;
        movementAudioSource.PlayOneShot(audioClip);
    }
    public void PlayCurrentEndReload(float volume = 1.0f, float pitch = 1.0f)
    {
        weaponAudioSource.Stop();
        weaponAudioSource.volume = volume;
        weaponAudioSource.pitch = pitch;
        weaponAudioSource.PlayOneShot(currentEndReload);
    }
    public void UpdateWeaponSound(WeaponItem weapon)
    {
        currentWeaponShoot = weapon.template.shootSound;
        currentStartReload = weapon.template.reloadStartSound;
        currentEndReload = weapon.template.reloadEndSound;
         
    }
    
    public void RequestSoundEmission(SoundType soundType)
    {
        
        if (IsServer)
        {
            SoundEmissionClientRpc((byte)soundType);
        }
        else
        {
            SoundEmissionServerRpc((byte)soundType);
        }
        
    }
    [ServerRpc]
    public void SoundEmissionServerRpc(byte soundType)
    {
        SoundEmissionClientRpc(soundType);
    }

    [ClientRpc]
    public void SoundEmissionClientRpc(byte soundType)
    {
        if (IsOwner)return;
        Debug.Log("Called");
        SoundType soundTypeSelected = (SoundType)soundType;
        switch (soundTypeSelected)
        {
            case SoundType.Action:
                break;
            case SoundType.Shoot:
                PlayCurrentShoot();
                Debug.Log("shoot");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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

public enum SoundType
{
    Action,
    Shoot, 
}
