using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour,INetObjectToClean
{

    public bool shutingDown { get; set; }
    public static GameController instance;

    [Header("Lobby")]
    public NetworkVariable<bool> started;
    public NetworkVariable<float> netTimeToStart = new NetworkVariable<float>();
    public float waitingTime;

    [Header("MapLogic")] 
    public float currentFarmStageTimer=0;
    public List<Transform> players=new List<Transform>();
    public NetworkVariable<MapLogic> mapLogic = new NetworkVariable<MapLogic>();
    public NetworkVariable<int> numberOfPlayers = new NetworkVariable<int>();
    public NetworkVariable<int> numberOfPlayersAlive = new NetworkVariable<int>();
    public NetworkVariable <Vector3> randomPoint = new NetworkVariable<Vector3>();
    public Transform sphereRadiusMesh;

    public Transform mapCenter;
    public float mapLimitRadius;
    public int timeToFarm=120;
    public float reduceZoneSpeed=2.0f;
    public float zoneRadius=2.0f;
    public int respawnTime=5;
    
    [Header("References")]
    [SerializeField] private CoinBehaivor coinPrefab;
    public CapsuleCollider sphereRadius;
    
    [Header("Zones")]
    public Transform[] spawnPoints;
    public Transform zoneInstances;
    public PlayerZoneController zoneControllerPrefab;
    public List<PlayerZoneController> zoneControllers;
    zoneColors[] zoneColors;


    private void OnDrawGizmos()
    {
        Gizmos.color= Color.red;
        Gizmos.DrawWireSphere(mapCenter.position, mapLimitRadius);
    }

    private void OnEnable()
    {
        // CoinBehaivor.OnCoinCollected += MoveCoin;
        if (instance == null)
        {
            instance = this;
            
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
        
    }


    
    void Start()
    {
        
        NetworkManager.Singleton.OnClientConnectedCallback += HandleConnection;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnection;
        CleanerController.instance.AddObjectToList(GetComponent<INetObjectToClean>());
    }

    public void LoadTutorialOptions()
    {
        SetMapLogicClientServerRpc(numberOfPlayers.Value, numberOfPlayersAlive.Value, reduceZoneSpeed, timeToFarm, 3, zoneRadius);
    }

    public void Initialize()
    {
        if (IsServer)
        {
            numberOfPlayers.Value = 1;
            numberOfPlayersAlive.Value = 1;
        }
    }
    public async void GetPlayerRef()
    {
        
        if (NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()==null) await Task.Yield();
        GameManager.Instance.localPlayerRef = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()
            .GetComponent<PlayerController>();
    }

    public override void OnNetworkSpawn()
    {
        Initialize();
        LoadGameOptions();
    }

    public void LoadGameOptions()
    {


        if (GameManager.Instance.localPlayerRef==null)
        {
            GetPlayerRef();
        }
        if (IsServer)
        {
            SetMapLogicClientServerRpc(numberOfPlayers.Value, numberOfPlayersAlive.Value, reduceZoneSpeed, timeToFarm, 3, zoneRadius);
        }
    }

    public void HandleConnection(ulong clientId)
    {
        Debug.Log("Connected");
        if (IsServer) {
            AddPlayerToListClientRpc();
        }
        if (IsClient && IsOwner)
        {
            OnPlayerEnterServerRpc();
            SetNumberOfPlayerListServerRpc(clientId);
        }
    }


    public void HandleDisconnection(ulong clientId)
    {
        Debug.Log("Disconnected");
         if (IsServer)
         {
           AddPlayerToListClientRpc();
           OnPlayerOutServerRpc();
           //TODO: disconnect all clients
         }
         if (IsClient && IsOwner&& !IsServer)
         {
           SetMapLogicClientServerRpc(numberOfPlayers.Value, numberOfPlayersAlive.Value, reduceZoneSpeed, timeToFarm, 2, zoneRadius);
           SetNumberOfPlayerListServerRpc(clientId);
           GameManager.Instance.LoadMenuScene();
         }       
    }
    
    void Update()
    {

        if (shutingDown)return;
        if (!GameManager.Instance.isOnTutorial)
        {
            UpdateTime();
        }

        
        
    }

    private void FixedUpdate()
    {
        if (mapLogic.Value.isBattleRoyale && sphereRadius.radius>0)
        {
            ReduceSphereSize();
        }
    }


    public void OnPlayerDead(int index)
    {
         SetPlayerPosOnDeadClientRpc(zoneControllers[index].playerSpawn.position, index);
    }
    /// <summary>
    /// create the player zones with the player transforms
    /// </summary>
    public void StartGame()
    {
        NetworkingHandling.HostManager.instance.CloseLobby();
        AudioManager.instance.PlayNewStage();
        started.Value = true;
        for (int i = 0; i < numberOfPlayers.Value; i++)
        {

            if (IsOwner)
            {

                CreateZonesOnNet(i);

            }

            if (IsServer)
            {
                SetPlayerPosClientRpc(zoneControllers[i].playerSpawn.position, i);
            }
        }
        // if (IsServer) SpawnCoins();
    }

    public void CreateZonesOnNet(int index)
    {
        if (IsServer)
        {
            PlayerZoneController playerZoneController = Instantiate(zoneControllerPrefab, spawnPoints[index].position, Quaternion.identity, zoneInstances);
            playerZoneController.enemiesSpawnRate = mapLogic.Value.enemiesSpawnRate;

            playerZoneController.isBattleRoyale = mapLogic.Value.isBattleRoyale;
            playerZoneController.GetComponent<NetworkObject>().Spawn();
            playerZoneController.SetZone(index);
            AddZoneControllerClientRpc(playerZoneController.GetComponent<NetworkObject>().NetworkObjectId);
            SetZoneToPlayerClientRpc(index);
        }
        else
        {
            SpawnZonesInNetworkServerRpc(index);
        }
    }

    private void UpdateTime()
    {
        // if (numberOfPlayers.Value<2)return;
        if (IsServer && !started.Value)
        {
            netTimeToStart.Value += Time.deltaTime;

        }
        else if (IsClient && !started.Value && IsOwner)
        {
            SetTimeToStartServerRpc(Time.deltaTime);
        }
        if (netTimeToStart.Value > waitingTime && !started.Value)
        {
            StartGame();
            
        }
        if (started.Value && !mapLogic.Value.isBattleRoyale)
        {
            currentFarmStageTimer += Time.deltaTime;
            if (currentFarmStageTimer >= mapLogic.Value.totalTime) {
                
                if (IsServer)
                {
                    mapLogic.Value.isBattleRoyale = true;
                    SendMapBattleRoyaleValueClientRpc(true);
                    AudioManager.instance.PlayNewStage();
                }
                
            }
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPointsOnKillerServerRpc(int points, ulong instigatorClientId,ServerRpcParams serverRpcParams = default)
    {
        
        NetworkManager.Singleton.ConnectedClients[instigatorClientId].PlayerObject.GetComponent<PlayerStatsController>().AddAvaliblePoint(points); 
        
    }
    
   
    [ServerRpc(RequireOwnership = false)]
    public void NotifyKillServerRpc(ulong instigatorClientId,ServerRpcParams serverRpcParams = default)
    {
        PlayerStatsController playerStatsController = NetworkManager.Singleton.ConnectedClients[instigatorClientId].PlayerObject.GetComponent<PlayerStatsController>(); 
        playerStatsController.NotifyKillServerRpc(true); 
        
    }
     
    public Vector3 GetRandomPointFromCollider(Collider col)
    {
        Vector3 newPos=new Vector3(UnityEngine.Random.Range(col.bounds.min.x, col.bounds.max.x), 2.5f ,UnityEngine.Random.Range(col.bounds.min.z, col.bounds.max.z));
        return newPos;
    }
    public void ReduceSphereSize()
    {
        if (sphereRadius.radius>0)
        {
            sphereRadius.radius -= mapLogic.Value.zoneRadiusExpandSpeed * Time.fixedDeltaTime;
            float currentRadius = sphereRadius.radius * 2;
            sphereRadiusMesh.localScale = new Vector3(currentRadius,sphereRadiusMesh.localScale.y,currentRadius) ;
        }
    }
    #region ServerRpc
    [ServerRpc]
    public void SetTimeToStartServerRpc(float time)
    {
        netTimeToStart.Value += time;
    }

    [ServerRpc]
    public void SetMapLogicClientServerRpc(int numberOfPlayers,int numberOfPlayersAlive,float zoneRadiusExpandSpeed,int totalTime,float enemiesSpawnRate,float zoneRadius)
    {
        mapLogic.Value.SetMap(numberOfPlayers, numberOfPlayersAlive, zoneRadiusExpandSpeed, totalTime, enemiesSpawnRate, zoneRadius);
    }

    [ServerRpc]
    public void SetNumberOfPlayerListServerRpc(ulong clientId) {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
        {
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStatsController>().zoneAsigned.Value = (zoneColors)i;

        }
    }
    [ServerRpc]
    public void SetTimeLeftOnClientsServerRpc(ulong clientId)
    {
        for (ulong i = 0; i < (ulong)NetworkManager.Singleton.ConnectedClients.Count; i++)
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            player.GetComponentInChildren<Canvas>().GetComponentInChildren<TextMeshProUGUI>().text = "Player" + i+1;
        }

    }
    [ServerRpc]
    public void OnPlayerEnterServerRpc()
    {
          numberOfPlayersAlive.Value++;
          numberOfPlayers.Value++;
    }
    [ServerRpc]
    public void OnPlayerOutServerRpc()
    {
        
        numberOfPlayersAlive.Value--;
        numberOfPlayers.Value--;
    }


    /// <summary>
    /// Spawn the zones on the network
    /// </summary>
    [ServerRpc]
    public void SpawnZonesInNetworkServerRpc(int index)
    {
        PlayerZoneController playerZoneController = Instantiate(zoneControllerPrefab, spawnPoints[index].position, Quaternion.identity, zoneInstances);
        playerZoneController.enemiesSpawnRate = mapLogic.Value.enemiesSpawnRate;
        playerZoneController.SetZone(index);
        zoneControllers.Add(playerZoneController);
        zoneControllers[index].isBattleRoyale = false;
        zoneControllers[index].GetComponent<NetworkObject>().Spawn();
        
    }
    [ServerRpc]
    public void SetPlayerPosServerRpc(Vector3 pos, int playerIndex)
    {
        players[playerIndex].position = pos;
    }
    #endregion


    #region clientRpc
    [ClientRpc]
    public void SetPlayerPosClientRpc(Vector3 pos, int playerIndex)
    {
        players[playerIndex].GetComponent<PlayerController>().characterController.enabled = false;
        players[playerIndex].position = pos;
        players[playerIndex].GetComponent<PlayerController>().characterController.enabled = true;
        players[playerIndex].GetComponent<PlayerStatsController>().OnStatsChanged?.Invoke();
        
        // players[playerIndex].GetComponent<PlayerStatsController>().SetHealth( players[playerIndex].GetComponent<PlayerStatsController>().GetMaxHealth());

        Debug.Log("Called on client");
    }
    
    [ClientRpc]
    public void SetPlayerPosOnDeadClientRpc(Vector3 pos, int playerIndex)
    {
        PlayerController playerController = players[playerIndex].GetComponent<PlayerController>();
        playerController.characterController.enabled = false;
        players[playerIndex].position = pos;
        playerController.characterController.enabled = true;
        playerController.playerStats.OnStatsChanged?.Invoke();

        Debug.Log("Called on client number" + playerIndex);
        Debug.Log("Killer: " + playerController.playerStats.clientIdInstigator.Value);

        
        //this is advicing the killed that the user is dead
        var rpcParams = new ServerRpcParams { };
        rpcParams.Receive.SenderClientId=playerController.playerStats.clientIdInstigator.Value;
        AddPointsOnKillerServerRpc(1, playerController.playerStats.clientIdInstigator.Value,rpcParams);            
        NotifyKillServerRpc(playerController.playerStats.clientIdInstigator.Value,rpcParams);     
      
            
        if (PlayerVFXController.respawningEffectHandle!=null)
        {
            PlayerVFXController.respawningEffectHandle.CreateVFX(players[playerIndex].transform.position, Quaternion.identity, false);
        }
        Debug.Log("Called on client");
    }
    
    [ClientRpc]
    public void SendMapBattleRoyaleValueClientRpc(bool val)
    {
        mapLogic.Value.isBattleRoyale = val;
    }

    
    [ClientRpc]
    public void AddPlayerToListClientRpc()
    {
        players.Clear();
        GameObject[] playersInScene = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject index in playersInScene)
        {
            if (index.GetComponent<NetworkObject>().IsSpawned)
            {
                players.Add(index.transform);
            }
            Debug.Log("Connected Client");
        }
    }
    
    [ClientRpc]
    public void SetCoinRandomPointClientRpc(Vector3 pos, ulong networkID)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkID].GetComponent<Transform>().position = pos;
    }

    //Set coins on the player
    [ClientRpc]
    public void SetCoinsOnClientRpc(int coinSpawned, int coinIndex, ulong networkID)
    {
        //set the coin on the client side   
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkID].GetComponent<Transform>().position = zoneControllers[coinIndex].spawnCoinPoint.position;
        zoneControllers[coinIndex].currentCoin = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkID].GetComponent<CoinBehaivor>();
        // players[coinIndex].GetComponent<PlayerStatsController>().coinPosition = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkID].GetComponent<CoinBehaivor>().transform;
    }
    //set the zone controller on the client
    [ClientRpc]
    public void AddZoneControllerClientRpc(ulong networkObjectId)
    {
        zoneControllers.Add(NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].GetComponent<PlayerZoneController>());
    }

    [ClientRpc]
    public void SetZoneToPlayerClientRpc(int index)
    {
        players[index].GetComponent<PlayerStatsController>().playerZoneController = zoneControllers[index];
    }
    #endregion

    public void CleanData()
    {
        if (IsOwner)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleConnection;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleDisconnection;
        }
    }

    public void OnSpawn()
    {
    }

}

public enum zoneColors
{
    red,
    blue,
    green,
    yellow,
    purple,
    orange,
    pink,
    brown,
}


