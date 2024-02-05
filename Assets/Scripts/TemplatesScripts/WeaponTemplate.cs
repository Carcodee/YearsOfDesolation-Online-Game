using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WeaponTemplate", menuName = "ScriptableObjects/WeaponsScriptableObj", order = 1)]
public class WeaponTemplate : ScriptableObject
{
    public Sprite weaponImage;
    public AmmoTypeTemplate ammoType;
    public string weaponName;
    public float shootRate;
    public float shootTimer;
    public float shootRefraction;
    public float currentShootRefraction;
    public float minShootRefraction;
    
}
