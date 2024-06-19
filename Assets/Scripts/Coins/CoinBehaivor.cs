using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CoinBehaivor : MonoBehaviour
{
    public NetworkVariable<ulong> networkPlayerID = new NetworkVariable<ulong>();
    public NetworkVariable <int> zoneAssigned;

    public static Action<CoinBehaivor> OnCoinCollected;
    public GameObject coinEffectPrefab;
    public PlayerStatsController playerStatsController;
    public Transform magPosition;
    public int currentPos;

    void Start()
    {

    }


    void Update()
    {
        if (playerStatsController!= null) {
            // if (playerStatsController.OwnerClientId != networkPlayerID.Value && IsServer) {
            //     gameObject.SetActive(false);
            // }
        }
        

    }
    

    private void OnTriggerEnter(Collider other)
    {
        // if (IsServer)
        // {
            if (other.TryGetComponent(out PlayerStatsController playerRef))
            {

                    //something happens
                    // playerRef.OnLevelUp?.Invoke();
                    // playerRef.RefillAmmo();
                    Instantiate(coinEffectPrefab, transform.position, Quaternion.identity);
                    Debug.Log("Coin Collected" + "Player Level: " + playerRef.GetLevel());
                
            }



    }
    //
    // [ClientRpc]
    // public void CoinCollectedClientRpc()
    // {
    //     OnCoinCollected?.Invoke(gameObject.GetComponent<CoinBehaivor>());
    //
    // }
}
