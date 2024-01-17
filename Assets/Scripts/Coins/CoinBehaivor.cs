using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CoinBehaivor : NetworkBehaviour
{
    public NetworkVariable<ulong> networkPlayerID = new NetworkVariable<ulong>();
    public NetworkVariable <int> zoneAssigned;

    public static Action<CoinBehaivor> OnCoinCollected;
    public GameObject coinEffectPrefab;
    public PlayerStatsController playerStatsController;

    void Start()
    {
        //playerStatsController= NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkPlayerID.Value].GetComponent<PlayerStatsController>();

    }


    void Update()
    {

    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.TryGetComponent(out PlayerStatsController playerRef))
            {
                if (playerRef.OwnerClientId == networkPlayerID.Value)
                {
                    //something happens
                    playerRef.OnLevelUp?.Invoke();
                    playerRef.RefillAmmo();
                    Instantiate(coinEffectPrefab, transform.position, Quaternion.identity);
                    Debug.Log("Coin Collected" + "Player Level: " + playerRef.GetLevel());


                }
            }
        }
        else
        {
            if (other.TryGetComponent(out PlayerStatsController playerRef)) {
                if (playerRef.OwnerClientId == networkPlayerID.Value) {
                    //something happens
                    Instantiate(coinEffectPrefab, transform.position, Quaternion.identity);
                    Debug.Log("Coin Collected"+ "Player Level: "+ playerRef.GetLevel());
                }
            }
        }



    }

    [ClientRpc]
    public void CoinCollectedClientRpc()
    {
        OnCoinCollected?.Invoke(gameObject.GetComponent<CoinBehaivor>());
    }
}
