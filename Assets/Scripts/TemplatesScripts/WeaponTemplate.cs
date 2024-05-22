using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WeaponTemplate", menuName = "ScriptableObjects/WeaponsScriptableObj", order = 1)]
public class WeaponTemplate : ScriptableObject
{
    public Sprite weaponImage;
    public WeaponAnimations weaponAnimationState;
    public AmmoTypeTemplate ammoType;
    public WeaponObjectController weaponObjectController;
    public AudioClip shootSound;
    public AudioClip reloadStartSound;
    public AudioClip reloadEndSound;
    public float weaponDamage;
    public string weaponName;
    public float shootRate;
    public float shootTimer;
    public float shootRefraction;
    public float currentShootRefraction;
    public float minShootRefraction;
    
}
[System.Serializable]
public struct WeaponAnimations
{
    
    public string LayerName;
    public string weaponChange;
    public string weaponShoot;
    public string weaponReload;
    public string WeaponSprint;
}