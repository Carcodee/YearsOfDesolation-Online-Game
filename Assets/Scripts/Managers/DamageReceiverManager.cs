using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiverManager : MonoBehaviour
{
    
    public static DamageReceiverManager instance;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public HitData CheckHitType(LayerMask layerMask)
    {
        string layerName = LayerMask.LayerToName(layerMask);
        if (Enum.TryParse(layerName, out hitType hitType))
        {
            switch (hitType)
            {
                case hitType.Head:
                    return new HitData { damageAmplifier = 2, hitType = hitType.Head };
                case hitType.Chest:
                    return new HitData { damageAmplifier = 1, hitType = hitType.Chest };
                case hitType.Legs:
                    return new HitData { damageAmplifier = 1 , hitType = hitType.Legs };
            }
          
        }
        Debug.LogError("LayerMask not found");
        return new HitData { damageAmplifier = 1, hitType = hitType.Chest };

    }
    

    void Update()
    {
        
    }
}

public struct HitData
{
    public float damageAmplifier;
    public hitType hitType;
    
    
}
public enum hitType
{
    Head,
    Chest,
    Legs
}
