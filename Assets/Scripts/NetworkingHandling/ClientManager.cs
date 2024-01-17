using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace NetworkingHandling
{
    public class ClientManager: MonoBehaviour
    {
        public static ClientManager instance;
        private Allocation _allocation;
        public MyAllocation myAllocation;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = this;
        }

        public async Task SetUpAllocation(string joinCode, UnityTransport transport)
        {
            JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinCode);
            myAllocation = new MyAllocation()
            {
                ipAddress = a.RelayServer.IpV4,
                port = (ushort)a.RelayServer.Port,
                allocationId = a.AllocationIdBytes,
                key = a.Key,
                connectionData = a.ConnectionData
            };
            transport.SetClientRelayData(myAllocation.ipAddress, myAllocation.port, myAllocation.allocationId, myAllocation.key, myAllocation.connectionData, a.HostConnectionData);
            
        }
        
        public async Task StartClient(string hostCode, UnityTransport transport)
        {
            await SetUpAllocation(hostCode, transport);
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
            }
        }

    }


    
}
