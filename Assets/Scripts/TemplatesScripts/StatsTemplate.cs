using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatsTemplate", menuName = "ScriptableObjects/StatsTemplate", order = 1)]
public class StatsTemplate : ScriptableObject
{
    public string preset;
    public int haste;
    public int health;
    public int stamina; 
    public int damage;
    public int armor;
    public int speed;

}
