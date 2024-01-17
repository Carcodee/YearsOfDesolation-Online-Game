using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;

public class EnemyController : NetworkBehaviour
{
    [Header("Internal References")]
    private EnemyBase enemyBase;
    private EnemyState state;
    private NavMeshAgent navMeshAgent;

    [Header("Refs")]
    public Transform target;

    void Start()
    {
        if (IsOwner)
        {
            enemyBase = GetComponent<EnemyBase>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    void Update()
    {
        ChangeEnemyState(EnemyState.Chase);
    }

    private void FixedUpdate()
    {
        
    }

    public void ChangeEnemyState(EnemyState newState)
    {
        state = newState;

        switch (state)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Chase:
                navMeshAgent.SetDestination(target.position);
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Dead:
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }
}
