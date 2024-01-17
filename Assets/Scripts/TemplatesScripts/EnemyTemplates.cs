using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatsTemplate", menuName = "ScriptableObjects/EnemyTemplate", order = 1)]
public class EnemyTemplates : ScriptableObject
{
    public int health;
    public int damage;
    public int armor;
    public float speed;
    public float attackSpeed;
    public int onKilledExp;

}
