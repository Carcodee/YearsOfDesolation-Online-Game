using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AmmoTemplate", menuName = "ScriptableObjects/AmmoScriptableObj", order = 2)]
public class AmmoTypeTemplate : ScriptableObject
{

    public int totalAmmo;
    public int currentBullets;
    public int totalBullets;
    public bool isReloading;
    public float reloadTime;
    public float reloadCurrentTime;
}
