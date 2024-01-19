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

    }


    void Update()
    {
        if (playerStatsController!= null) {
            if (playerStatsController.OwnerClientId != networkPlayerID.Value && IsServer) {
                gameObject.SetActive(false);
            }
        }
        

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
                    CoinCollectedClientRpc();
                    Debug.Log("Coin Collected" + "Player Level: " + playerRef.GetLevel());


                }
            }
        } else {
            //if (other.TryGetComponent(out PlayerStatsController playerRef)) {
            //    if (playerRef.OwnerClientId == networkPlayerID.Value) {
            //        //something happens
            //        CoinCollectedClientRpc();

            //        Debug.Log("Coin Collected" + "Player Level: " + playerRef.GetLevel());
            //    }
            //}
        }



    }

    [ClientRpc]
    public void CoinCollectedClientRpc()
    {
        OnCoinCollected?.Invoke(gameObject.GetComponent<CoinBehaivor>());
        Instantiate(coinEffectPrefab, transform.position, Quaternion.identity);

    }
}
