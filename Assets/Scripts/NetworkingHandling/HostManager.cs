using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace NetworkingHandling
{
    public class HostManager : MonoBehaviour
    {
        public static HostManager instance;
        public string lobbyName;
        public string lobbyId;
        public string hostCode;

        public MyAllocation myAllocation=new MyAllocation();

        private Allocation _allocation;
        public string region = "europe-west2";
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;

        }
        public void SetRelayTransport(UnityTransport transport)
        {
            transport.SetHostRelayData(myAllocation.ipAddress, myAllocation.port, myAllocation.allocationId, myAllocation.key, myAllocation.connectionData);
        }
        public async Task SetAllocation(UnityTransport transport)
        {
            _allocation=await RelayService.Instance.CreateAllocationAsync(8,region);
            myAllocation = new MyAllocation()
            {
                ipAddress = _allocation.RelayServer.IpV4,
                port = (ushort)_allocation.RelayServer.Port,
                allocationId = _allocation.AllocationIdBytes,
                key = _allocation.Key,
                connectionData = _allocation.ConnectionData
            };
            hostCode= await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            SetRelayTransport(transport);
        }
        public async Task StartHost()
        {
            
            try
            {
                var createLobbyOptions = new CreateLobbyOptions();
                createLobbyOptions.IsPrivate = false;
                createLobbyOptions.Data = new Dictionary<string, DataObject>()
                {
                    {
                        "JoinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: hostCode
                        )
                    }
                };
                Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, 8, createLobbyOptions);
                lobbyId= lobby.Id;
                StartCoroutine(Heartbeat(15));


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        
            NetworkManager.Singleton.StartHost();
        }
        public IEnumerator Heartbeat(float waitTime)
        {
            var delay= new WaitForSeconds(waitTime);

            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }
        
    }
}

public struct MyAllocation
{
    public string ipAddress;
    public ushort port;
    public byte[] allocationId;
    public byte[] key;
    public byte[] connectionData; 
}