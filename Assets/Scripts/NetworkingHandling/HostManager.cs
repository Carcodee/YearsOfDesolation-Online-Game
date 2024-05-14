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
        public int maxNumberOfPlayers = 8;

        public MyAllocation myAllocation=new MyAllocation();

        private Allocation _allocation;
        public string region = "europe-west2";
        public UnityTransport currentTransport;
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
            currentTransport.SetHostRelayData(myAllocation.ipAddress, myAllocation.port, myAllocation.allocationId, myAllocation.key, myAllocation.connectionData);
            
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
            currentTransport = transport;
            SetRelayTransport(transport);
        }

        public async Task DisconnectHost()
        {
            
            StopAllCoroutines();
            CleanerController.instance.StopLogic(true);
            CleanerController.instance.Clean();
            foreach (Transform player in GameController.instance.players)
            {
                NetworkObject playerRef= player.GetComponent<NetworkObject>();
                if (playerRef.OwnerClientId==0)
                {
                    continue;
                }
                NetworkManager.Singleton.DisconnectClient(playerRef.OwnerClientId);
                Debug.Log("ID: "+ playerRef.OwnerClientId);
            }

            GameManager.Instance.localPlayerRef = null;
            NetworkManager.Singleton.Shutdown();
            currentTransport.Shutdown();
            var deleteLobbyAsync = Lobbies.Instance.DeleteLobbyAsync(lobbyId);
            Destroy(GameController.instance);
            if (NetworkManager.Singleton.ShutdownInProgress && !deleteLobbyAsync.IsCompleted) await Task.Yield();
            GameManager.Instance.LoadMenuScene();
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
            GameManager.Instance.CreateController();
        }

        public async void CloseLobby()
        {
            UpdateLobbyOptions lobbyOptions = new UpdateLobbyOptions();
            lobbyOptions.IsLocked = true;
            
            var lobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
 
            lobbyOptions.Name =lobby.Name;
            Lobbies.Instance.UpdateLobbyAsync(lobbyId, lobbyOptions);
        }

        public void StartHostNoLobby()
        {
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