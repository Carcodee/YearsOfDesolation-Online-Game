using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyBase : NetworkBehaviour,IDamageable
{
    public EnemyTemplates template;
    public int health;
    public int damage;
    public int armor;
    public float speed;
    public float attackSpeed;
    public int onKilledExp;
    public IDamageable iDamageable;



    void Start()
    {
        if (IsOwner)
        {
            iDamageable = GetComponent<IDamageable>();
        }
    }

    void Update()
    {
        
    }
    public void InitializateTemplate()
    {

        health = template.health;
        damage = template.damage;
        armor = template.armor;
        speed = template.speed;
        attackSpeed = template.attackSpeed;
        onKilledExp = template.onKilledExp;
    }
    public void TakeDamage(int damage)
    {
        health = health - damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
