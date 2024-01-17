using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{

    public static GameController instance;

    [Header("Lobby")]
    public bool started;
    public NetworkVariable<float> netTimeToStart = new NetworkVariable<float>();
    public float waitingTime;

    [Header("MapLogic")]
    public float farmStageTimer;
    public List<Transform> players=new List<Transform>();
    public NetworkVariable<MapLogic> mapLogic = new NetworkVariable<MapLogic>();
    public NetworkVariable<int> numberOfPlayers = new NetworkVariable<int>();
    public NetworkVariable<int> numberOfPlayersAlive = new NetworkVariable<int>();
    public NetworkVariable <Vector3> randomPoint = new NetworkVariable<Vector3>();

    public int respawnTime=5;
    
    [Header("References")]
    [SerializeField] private CoinBehaivor coinPrefab;
    public SphereCollider sphereRadius;
    [Header("Zones")]
    public Transform[] spawnPoints;
    public Transform zoneInstances;
    public PlayerZoneController zoneControllerPrefab;
    public List<PlayerZoneController> zoneControllers;
    zoneColors[] zoneColors;
    
    private void Awake()
    {
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
    private void OnEnable()
    {
        CoinBehaivor.OnCoinCollected += MoveCoin;
    }
    private void OnDisable()
    {
        CoinBehaivor.OnCoinCollected -= MoveCoin;

    }

    void Start()
    {

        //Check if a player connected to the server
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (IsServer) {
                AddPlayerToListClientRpc();
            }

            if (IsClient && IsOwner)
            {
                OnPlayerEnterServerRpc();
                SetMapLogicClientServerRpc(numberOfPlayers.Value, numberOfPlayersAlive.Value, 4f, 25, 3, 5);
                SetNumberOfPlayerListServerRpc(clientId);
            }

        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {

            if (IsServer)
            {
                AddPlayerToListClientRpc();
            }

            if (IsClient && IsOwner)
            {

                OnPlayerOutServerRpc();
                SetMapLogicClientServerRpc(numberOfPlayers.Value, numberOfPlayersAlive.Value, 5.0f, 25, 3, 5);
                SetNumberOfPlayerListServerRpc(clientId);

            }

        };



    }

    void Update()
    {

        UpdateTime();
        if (mapLogic.Value.isBattleRoyale && sphereRadius.radius>0)
        {
                ReduceSphereSize();
        }
        
    }


    public void OnPlayerDead(int index)
    {
         SetPlayerPosClientRpc(zoneControllers[index].playerSpawn.position, index);
    }
    /// <summary>
    /// create the player zones with the player transforms
    /// </summary>
    public void StartGame()
    {
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
            if (IsServer) SpawnCoins();
            started = true;
    }


    /// <summary>
    /// Set Each player a zone
    /// </summary>
    public void CreatePlayerZones()
    {
        zoneColors = new zoneColors[numberOfPlayers.Value];
        for (int i = 0; i < zoneColors.Length; i++)
        {
            zoneColors[i] = (zoneColors)i;
        }
       
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


    public void SpawnCoins()
    {
            int coinSpawned = 0;
            int coinIndex = 0;
            while (coinSpawned <  numberOfPlayers.Value)
            {
                if(coinIndex > numberOfPlayers.Value-1 || coinSpawned>numberOfPlayers.Value-1)break;
                if (coinSpawned != (int)zoneControllers[coinIndex].zoneAsigned.Value && zoneControllers[coinIndex].currentCoin==null)
                {
                    CoinBehaivor myCoin = Instantiate(coinPrefab, zoneControllers[coinSpawned].spawnCoinPoint.position, Quaternion.identity);

                    myCoin.GetComponent<NetworkObject>().Spawn();
                    //this represent who owns the coin
                    myCoin.networkPlayerID.Value = players[coinSpawned].GetComponent<PlayerStatsController>().OwnerClientId;
                    //this represent the zone where the coin is
                    //myCoin.transform.position = zoneControllers[coinIndex].spawnCoinPoint.position;
                    myCoin.zoneAssigned.Value =coinSpawned;
                    SetCoinsOnClientRpc(coinSpawned, coinIndex, myCoin.GetComponent<NetworkObject>().NetworkObjectId);

                    coinSpawned++;

                    coinIndex = 0;
                    continue;
                }

                coinIndex++;
            }

    }
    public void MoveCoin(CoinBehaivor coin)
    {
        for (int i = 0; i < zoneControllers.Count(); i++)
        {
            if ((int)zoneControllers[i].zoneAsigned.Value!= coin.zoneAssigned.Value)
            {
                if (IsServer)
                {
                    randomPoint.Value = GetRandomPointFromCollider(zoneControllers[i].GetComponentInChildren<Collider>());
                }
                ulong netId=  coin.GetComponent<NetworkObject>().NetworkObjectId;
                SetCoinRandomPointClientRpc(randomPoint.Value, netId);
            }
        }
    }


    private void UpdateTime()
    {
        if (IsServer && !started)
        {
            netTimeToStart.Value += Time.deltaTime;

        }
        else if (IsClient && !started && IsOwner)
        {
            SetTimeToStartServerRpc(Time.deltaTime);
        }
        if (netTimeToStart.Value > waitingTime && !started)
        {
            StartGame();
        }
        if (started && !mapLogic.Value.isBattleRoyale)
        {
            farmStageTimer += Time.deltaTime;
       
            if (farmStageTimer >= mapLogic.Value.totalTime)
            {
                if (IsServer)
                {
                    mapLogic.Value.isBattleRoyale = true;
                    SendMapBattleRoyaleValueClientRpc(true);
                }
            }
        }

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
            sphereRadius.radius -= mapLogic.Value.zoneRadiusExpandSpeed * Time.deltaTime;
        }
    }
    #region ServerRpc
    [ServerRpc]
    public void SetTimeToStartServerRpc(float time)
    {
        netTimeToStart.Value += time;
    }
    [ServerRpc]
    public void ReduceTotalTimeServerRpc(float val)
    {
        mapLogic.Value.totalTime -= val;
    }
    [ServerRpc]
    public void SetBattleRoyaleServerRpc(bool val)
    {
        if (IsOwner) mapLogic.Value.isBattleRoyale = val;
    }
    [ServerRpc]
    public void SetMapLogicClientServerRpc(int numberOfPlayers,int numberOfPlayersAlive,float zoneRadiusExpandSpeed,int totalTime,float enemiesSpawnRate,float zoneRadius)
    {
        Debug.Log("Called on client");
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
            players.Add(index.transform);
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
        players[coinIndex].GetComponent<PlayerStatsController>().coinPosition = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkID].GetComponent<CoinBehaivor>().transform;
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

