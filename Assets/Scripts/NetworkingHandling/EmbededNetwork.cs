using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EmbededNetwork : NetworkBehaviour
{
    public static EmbededNetwork instance;
    public static EmbededNetwork Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EmbededNetwork>();
            }
            return instance;
        }
    }
    public Action actionToCall;
    public List<Action<Vector3>> actionToCallAtPos = new List<Action<Vector3>>();
    
    /// <summary>
    /// ///////////////////////////////////////// SERVER RPC
    /// </summary>
    [ServerRpc]
    public void MyCustomServerRpc()
    {
        
        // This is the server RPC
        actionToCall?.Invoke();
    }
    [ServerRpc]
    public void MyCustomServerRpc(Vector3 pos, int id)
    {

        actionToCallAtPos[id]?.Invoke(pos);
    }
    [ServerRpc]
    public void CallMyCustomClient_ServerRPC()
    {

        MyCustomClientRpc();
    }
    [ServerRpc (RequireOwnership = false)]
    public void CallMyCustomClient_ServerRPC(Vector3 pos, int id)
    {

        MyCustomClientRpc(pos,id);
    }
    
    
    
    /// <summary>
    /// ///////////////////////////////// CLIENT RPC
    /// </summary>
    [ClientRpc]
    public void MyCustomClientRpc()
    {
        // This is the server RPC
        actionToCall?.Invoke();
    }
    [ClientRpc]
    public void MyCustomClientRpc(Vector3 pos, int id)
    {
        actionToCallAtPos[id]?.Invoke(pos);
    }

    public override void OnDestroy()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkDespawn()
    {

                
        base.OnNetworkDespawn();
        for (int i = 0; i < actionToCallAtPos.Count; i++)
        {
         actionToCallAtPos[i] = null;   
        }
        actionToCall = null;
    }
}
