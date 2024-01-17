using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerZoneController : NetworkBehaviour
{
    [Header("Zone variables")]
    public NetworkVariable<zoneColors> zoneAsigned = new NetworkVariable<zoneColors>();
    public float enemiesSpawnRate;
    public Transform spawnCoinPoint;
    public Transform playerSpawn;
    public Transform enemyContainer;
    public CoinBehaivor currentCoin;

    [Header("Ref")]
    public EnemyController enemyPrefab;
    
    public bool isBattleRoyale;

    [Header("Privates")]
    [SerializeField]private float internalSpawnTimer;
    int enemiesSpawned;
    public List<EnemyController> enemies;

    void Start()
    {
        
    }

    void Update()
    {
        if (!IsOwner) return;
        if(!isBattleRoyale)
        {


        }
    }

    public void SetZone(int val)
    {
        if (IsServer)
        {
            zoneAsigned.Value = (zoneColors)val;
        }
        else
        {
            SetZoneServerRpc(val);
        }
    }


    [ServerRpc]
    public void SetZoneServerRpc(int val)
    {
        zoneAsigned.Value = (zoneColors)val;
    }


    #region clientRpc

    [ClientRpc]
    public void SetPlayerOnClientRpc(int index)
    {
        //enemies[index].target = playerAssigned; 
    }
    #endregion
}

