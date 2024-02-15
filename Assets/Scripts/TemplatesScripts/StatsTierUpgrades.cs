using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeTiers", menuName = "ScriptableObjects/UpgradesPerTier", order = 1)]
public class StatsTierUpgrades : ScriptableObject 
{
    
    public int index;
    public UpgradeType upgradeType;
    public FlexibleValue value;
    
    [System.Serializable]
    public struct FlexibleValue
    {
        public float floatValue;
        public int intValue;
    }
    
}
