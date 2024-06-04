using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetworkingHandling;
using Players.PlayerStates;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public class PlayerStatsController : NetworkBehaviour, IDamageable, INetObjectToClean
{
    public UnityAction OnSpawnPlayer;
    public UnityAction OnStatsChanged;
    public Action OnLevelUp;    
    public Action OnPlayerDead;
    public Action <Transform> OnWeaponChanged;
    
    [Header("References")]
    public StatsTemplate[] statsTemplates;
    public NetworkVariable <int> statsTemplateSelected;
    public PlayerComponentsHandler playerComponentsHandler;
    public StateMachineController stateMachineController;
    public PlayerVFXController playerVFXController;
    public PlayerSoundController playerSoundController;
    public Transform takeDamagePosition;
    public Vector3 deadPosition;
    public PlayerController playerController;
    [Header("Stats")]
    [SerializeField] private NetworkVariable<int> haste = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<float> health = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<int> maxHealth = new NetworkVariable<int>();

    [SerializeField] private NetworkVariable<int> stamina = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<float> damage = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<int> armor = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<float> speed = new NetworkVariable<float>();

    [SerializeField] private NetworkVariable<int> playerLevel = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> avaliblePoints = new NetworkVariable<int>();
    [SerializeField] public  NetworkVariable<bool> enemyKilled = new NetworkVariable<bool>();
    [SerializeField] public  NetworkVariable<FixedString32Bytes> userName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>();
    [FormerlySerializedAs("StartGameHealth")] public float startGameHealth;
    public string[] statHolderNames;
    public int[] statHolder;
    public bool isPlayerInsideTheZone;
    public bool isPlayerInsideOfMap;
    public bool dangerSoundPlayed;
    public bool outsideOffZoneSoundPlayed;

    public string lastEnemyKilledName = "";
    public string instigatorName = "";
    [Header("WeaponSpawnPoints")]
    public Transform [] weaponSpawnPoint;
    
    [Header("Current Gamelogic")]
    public NetworkVariable<zoneColors> zoneAsigned=new NetworkVariable<zoneColors>();
    public CoinBehaivor coinPosition;
    public PlayerZoneController playerZoneController;
    public float currentOutsideTimerTick;
    public float outsideTimerTick=1;
    
    public float currentOutsideOfMapTimer= 0.0f;
    public float outsideOfMapTickTime = 8.0f;
    private bool _IsInitizalized=false;
    private bool _isInNetwork=false;
    public CoinBehaivor coin;
    
    
    [Header("Interfaces")]
    private IDamageable iDamageable;

    public bool shutingDown { get; set; }
    
    [Header("NetCode")]
    public NetworkObject playerObj;
    public PlayerController playerControllerKillerRef = null;
    
    [Header("PlayerBuild")]
    public PlayerBuild playerBuildSelected;
    public bool hasPlayerSelectedBuild=false;
    
    [Header("Weapons")]
    public WeaponItem currentWeaponSelected;
    public WeaponItem onBagWeapon;
    public WeaponItem ak47;
    public WeaponItem doublePistols;
    public WeaponTemplate [] weaponsData;
    public Action OnWeaponChange;
    public GameObject noBuildWeapon;
    public Transform weaponNoBuildGripPoint;

    //temporal variable
    public bool isChangingWeapon=false;
    
    [Header("Netcode")]
    public NetworkVariable<ulong> clientIdInstigator;

    private Transform playerTransform;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        stateMachineController=GetComponent<StateMachineController>();
        if (IsOwner)
        {
            Debug.Log("OnNetworkSpawn called. IsOwner: " + IsOwner);
            OnSpawnPlayer += InitializateStats;
            // OnStatsChanged += UpdateStats;
            OnLevelUp += LevelUp;
            OnLevelUp+=RefillAmmo;
            iDamageable = GetComponent<IDamageable>();
            isPlayerInsideTheZone = true;
            OnSpawnPlayer?.Invoke();
            _isInNetwork = true;
            INetObjectToClean[] objectToCleans = GetComponents<INetObjectToClean>();

        
            foreach (INetObjectToClean objectToClean in objectToCleans)
            {
                CleanerController.instance.AddObjectToList(objectToClean);
            }
        }
        PostProccesingManager.instance.DeactivateMenuBlur();
        
        
    }

    public override void OnNetworkDespawn()
    {
        if (!GameManager.Instance.gameEnded)
        {
            GameManager.Instance.DisconnectNotificationText = "Host lost connection";
        }
        CleanerController.instance.Clean();
    }

    private void Start()
    {
        stateMachineController=GetComponent<StateMachineController>();

        if (IsOwner)
        {
            Debug.Log("OnNetworkSpawn called. IsOwner: " + IsOwner);

            OnSpawnPlayer += InitializateStats;
            // OnStatsChanged += UpdateStats;
            OnLevelUp += RefillAmmo;
            OnLevelUp += LevelUp;
            iDamageable = GetComponent<IDamageable>();
            isPlayerInsideTheZone = true;
            isPlayerInsideOfMap = true;
            _isInNetwork = true;
            OnSpawnPlayer();

#if UNITY_EDITOR
            DebugManager.instance.playerStatsController = this;
#endif
            PostProccesingManager.instance.DeactivateMenuBlur();

        }
    }

    private void Update()
    {
        if (shutingDown)return;
        
        AllResourcesReady(b =>
        {
            GameManager.Instance.ActivateLoadingScreen(!b);
            GameManager.Instance.ActivateMenu(!b);
        });

        if (IsOwner) {
            if (!_IsInitizalized && health.Value<=0) {
                OnSpawnPlayer?.Invoke();
                _IsInitizalized = true;
            }

            if (stateMachineController.currentState.stateName=="Viewer")return;
            OutsideZoneDamage();
            HandleWeaponChange();

            
            if (GameController.instance.started.Value&& coinPosition==null)
            {
                SpawnPlayerCoin();
            }
        }
        //TODO: find some way to check if the player is dead with ownership

    }

    public async void AllResourcesReady(Action<bool> callback)
    {
        if (GameManager.Instance.ReadyToStart)return;
        await Task.Delay(500);
        GameManager.Instance.PlayerInGameSettings();
        if (AudioManager.instance.backGroundAudioSource.clip.loadState != AudioDataLoadState.Loaded) await Task.Yield();
        
        GameManager.Instance.ReadyToStart = true;
        PlayerComponentsHandler.IsCurrentDeviceMouse = false;
        callback.Invoke(true);
    }
    public void SelectBuild(PlayerBuild build)
    {
        playerBuildSelected = build;
        hasPlayerSelectedBuild = true;
        currentWeaponSelected = playerBuildSelected.first_weapon;
        onBagWeapon = playerBuildSelected.second_weapon;
        playerBuildSelected.CreateDataBuild();
        
        playerBuildSelected.first_weapon.weaponObjectController=Instantiate(playerBuildSelected.first_weapon.weapon.weaponObjectController, weaponSpawnPoint[0].position, Quaternion.identity, weaponSpawnPoint[0]);
        playerBuildSelected.first_weapon.weaponObjectController.transform.localPosition =  playerBuildSelected.first_weapon.weapon.weaponObjectController.transform.localPosition;
        playerBuildSelected.first_weapon.weaponObjectController.transform.localRotation= playerBuildSelected.first_weapon.weapon.weaponObjectController.transform.localRotation;
      
        playerBuildSelected.second_weapon.weaponObjectController=Instantiate(playerBuildSelected.second_weapon.weapon.weaponObjectController, weaponSpawnPoint[1].position, Quaternion.identity, weaponSpawnPoint[0]);
        playerBuildSelected.second_weapon.weaponObjectController.transform.localPosition = playerBuildSelected.second_weapon.weapon.weaponObjectController.transform.localPosition;
        playerBuildSelected.second_weapon.weaponObjectController.transform.localRotation = playerBuildSelected.second_weapon.weapon.weaponObjectController.transform.localRotation;

        playerBuildSelected.second_weapon.weaponObjectController.gameObject.SetActive(false);
        
        noBuildWeapon.SetActive(false);
        
        stateMachineController.SetChangingWeaponState(playerBuildSelected.first_weapon, "ChangingWeapon");
        currentWeaponSelected.weaponObjectController.gameObject.SetActive(true);
        onBagWeapon = playerBuildSelected.second_weapon;
        onBagWeapon.weaponObjectController.gameObject.SetActive(false);
        OnWeaponChanged.Invoke(currentWeaponSelected.weapon.weaponObjectController.weaponBulletSpawnPoints);
        playerSoundController.UpdateWeaponSound(currentWeaponSelected);
    }

    public void UpdateWeaponSound()
    {
    
    }
    void InitializateStats()
    {
        SetTemplate(0);
        playerTransform = transform;
        playerObj = GetComponent<NetworkObject>();
        if (playerSoundController==null)
        {
            playerSoundController = GetComponent<PlayerSoundController>();
        }
        if (statsTemplates[statsTemplateSelected.Value] == null)
        {
            Debug.LogError("StatsTemplate is null");
            return;
        }
        SetHasteServerRpc(statsTemplates[statsTemplateSelected.Value].haste);
        SetHealth(10);
        Debug.Log("Health called. hpValue: " + statsTemplates[statsTemplateSelected.Value].health);
        SetNameServerRpc(GameManager.Instance.localPlayerName);
        SetStaminaServerRpc(statsTemplates[statsTemplateSelected.Value].stamina);
        SetArmorServerRpc(statsTemplates[statsTemplateSelected.Value].armor);
        SetSpeedServerRpc(statsTemplates[statsTemplateSelected.Value].speed);
        SetMaxHealthServerRpc(10);
        startGameHealth = statsTemplates[statsTemplateSelected.Value].health;

        SetLevelServerRpc(1);
        SetAvaliblePointsServerRpc(0);
        health.OnValueChanged += SetPlayerOnPos;
        ak47 = new WeaponItem(weaponsData[(int)WeaponType.Ak99]);
        doublePistols = new WeaponItem(weaponsData[(int)WeaponType.Pistol]);
        SetWeapon(ak47);
        onBagWeapon = doublePistols;
        playerController = transform.GetComponent<PlayerController>();
        playerController.SetSpeedStateServerRpc(statsTemplates[statsTemplateSelected.Value].speed);
        playerSoundController.UpdateWeaponSound(currentWeaponSelected);
        OnStatsChanged?.Invoke();
    }
    public void SetWeapon(WeaponItem weapon)
    {
        currentWeaponSelected = weapon;
        SetDamageServerRpc(currentWeaponSelected.weapon.weaponDamage);

    }

    public void SpawnPlayerCoin()
    {
        if (GameController.instance.zoneControllers.Count<=1)return;
        
        int coinIndexZone = UnityEngine.Random.Range(0, GameController.instance.zoneControllers.Count);
        if (coinIndexZone==(int)zoneAsigned.Value)
        {
            SpawnPlayerCoin();
            return;
        }
        SpawnCoin(coin, GameController.instance.zoneControllers[coinIndexZone].transform.position);
    }

    public void SpawnCoin(CoinBehaivor coinPrefab, Vector3 pos)
    {
        coinPosition= Instantiate(coinPrefab, pos, quaternion.identity);
        
    }

    public void ShowPlayerKilled(string playerName)
    {
        CanvasController.OnEnemyKilled?.Invoke(playerName);
        
    }

    
    public void RefillAmmo()
    {
        currentWeaponSelected.weapon.ammoBehaviour.AddAmmo(60);
    }

    public void SetHealth(int value)
    {
        if (IsServer)
        { 
            health.Value = value;
        }
        else
        {
           SetHealthServerRpc(value);
        }
    }
    public void SetTemplate(int index)
    {
        if (IsServer)
        {
           statsTemplateSelected.Value = index;
        }
        else
        {
            SetTemplaterServerRpc(index);
        }
        
    }

    public void HandleWeaponChange()
    {
        if (!hasPlayerSelectedBuild)return;
        
        // if(!hasPlayerSelectedBuild)return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            
            if (currentWeaponSelected == playerBuildSelected.first_weapon) return;
            stateMachineController.SetChangingWeaponState(playerBuildSelected.first_weapon, "ChangingWeapon");
            currentWeaponSelected.weaponObjectController.gameObject.SetActive(true);
            onBagWeapon = playerBuildSelected.second_weapon;
            onBagWeapon.weaponObjectController.gameObject.SetActive(false);
            OnWeaponChanged.Invoke(currentWeaponSelected.weapon.weaponObjectController.weaponBulletSpawnPoints);
            playerSoundController.UpdateWeaponSound(currentWeaponSelected);
            F_In_F_Out_Obj.OnWeapontChangedAnim?.Invoke();
            CanvasController.OnReloadFinished?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (currentWeaponSelected == playerBuildSelected.second_weapon) return;
            stateMachineController.SetChangingWeaponState(playerBuildSelected.second_weapon,"ChangingWeapon");
            currentWeaponSelected.weaponObjectController.gameObject.SetActive(true);
            onBagWeapon = playerBuildSelected.first_weapon;
            onBagWeapon.weaponObjectController.gameObject.SetActive(false);
            OnWeaponChanged.Invoke(currentWeaponSelected.weapon.weaponObjectController.weaponBulletSpawnPoints);
            playerSoundController.UpdateWeaponSound(currentWeaponSelected);
            F_In_F_Out_Obj.OnWeapontChangedAnim?.Invoke();
            CanvasController.OnReloadFinished?.Invoke();

        }

    }
    public void OutsideZoneDamage()
    {
        if (Vector3.Distance(GameController.instance.mapCenter.position, playerTransform.position) > GameController.instance.mapLimitRadius)
        {
            currentOutsideOfMapTimer += Time.deltaTime;
            isPlayerInsideOfMap = false;
            if (!dangerSoundPlayed)
            {
                AudioManager.instance.DangerSound();
                dangerSoundPlayed = true;
            }
            PauseController.OnAlertActivated.Invoke(isPlayerInsideTheZone);
            PostProccesingManager.instance.ApplyPostProcessing(true);
            if (currentOutsideOfMapTimer>= outsideOfMapTickTime)
            {
                currentOutsideOfMapTimer = 0.0f;
                TakeDamage(maxHealth.Value);
            }
            return;
        }
        else
        {
            if (dangerSoundPlayed)
            {
                AudioManager.instance.UIAudioSource.Stop();
                dangerSoundPlayed = false;
            } 
            // PauseController.OnAlertActivated.Invoke(false);
            // PostProccesingManager.instance.ApplyPostProcessing(false);
            currentOutsideOfMapTimer = 0.0f;
            isPlayerInsideOfMap = true;
        }
        
        if (health.Value >= 0)
        {
            if (!isPlayerInsideTheZone)
            {
                if (!outsideOffZoneSoundPlayed)
                {
                    // playerSoundController.PlayActionSound(playerSoundController.outsideOffZoneDamage);
                    outsideOffZoneSoundPlayed = true;
                }
                currentOutsideTimerTick += Time.deltaTime;
                if (currentOutsideTimerTick > outsideTimerTick)
                {
                    currentOutsideTimerTick = 0;
                    TakeDamage(GameController.instance.mapLogic.Value.damagePerTick);
                }
                PostProccesingManager.instance.ApplyPostProcessing(true);

            }
            else
            {
                if (!outsideOffZoneSoundPlayed)
                {
                    // playerSoundController.actionsAudioSource.Stop();
                    outsideOffZoneSoundPlayed = false;
                }
                PostProccesingManager.instance.ApplyPostProcessing(false);
            }
        }

    }

    public void GoMenuOnDead()
    {
        if (health.Value<=0&& GameController.instance.mapLogic.Value.isBattleRoyale)
        {
            SceneManager.LoadScene("Menu");
            Destroy(gameObject);
        }
    }

    public void Disconnect()
    {
        if (IsServer) {
           NetworkingHandling.HostManager.instance.DisconnectHost();
        }
        else
        { 
            GameManager.Instance.gameEnded = true;
            ClientManager.instance.DisconnectClient(NetworkObject.OwnerClientId);
        }
 
    }
    public void TakeDamage(float damage)
    {
        if (IsOwner)
        {
            if ((stateMachineController.currentState.stateName == "Dead" && GameController.instance.mapLogic.Value.isBattleRoyale) || (health.Value-damage<=0 && GameController.instance.mapLogic.Value.isBattleRoyale))
            {
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (GameManager.Instance.isOnTutorial)
                {
                    Disconnect();
                    return;
                }
                Debug.Log("Dead");
                playerComponentsHandler.setViewer(clientIdInstigator.Value);
                playerController.DeactivatePlayer();
                playerController.NotifyPlayerDeactivatedServerRpc(OwnerClientId, false);
                stateMachineController.SetState("Viewer");
                // Destroy(gameObject);
                return;
            }

            if (stateMachineController.currentState.stateName == "Dead" || health.Value <= 0 ||
                stateMachineController.currentState.stateName == "Viewer")
            {
                return;
            }
            if (IsServer)
            {
                //this is wrong stat holder is controlling the health
                health.Value -= (damage);
                StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                playerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position,  Quaternion.identity,IsServer);
                
            }
            else
            {
                SetHealthServerRpc(health.Value - (damage));  
                StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                playerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position,  Quaternion.identity,IsServer);
            }
            playerVFXController.BodyDamageVFX();
            CanvasController.OnUpdateUI?.Invoke();
            playerSoundController.PlayActionSound(playerSoundController.damageTakeSound);
            OnStatsChanged?.Invoke();
        }
    }

    public void TakeDamageOffline(float damage)
    {
            //this is wrong stat holder is controlling the health
            health.Value -= (damage);
            if (health.Value<=0)
            { 
                Instantiate(playerVFXController.OnDeadEffectPrefab, transform.position, quaternion.identity);
                if (GameManager.Instance.isOnTutorial)
                {
                    TutorialStagesHandler.instance.SetTutorialStage(TutorialStage.ZoneComing);
                }
                Destroy(gameObject); 
                return;
            }
            playerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity,IsServer);
            playerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity,IsServer);
            playerSoundController.PlayActionSound(playerSoundController.killDoneSound);
            playerVFXController.BodyDamageVFX();
            // OnStatsChanged?.Invoke();
    }


    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int myDamage, ulong clientId, string userName)
    {
        //TODO> money is being added to the player that is dead
        TakeDamageClientRpc(myDamage, clientId, userName);
    }
    
    [ServerRpc]
    public void CallServerOnDeadServerRpc(int zoneAssigned)
    {
        GameController.instance.OnPlayerDead(zoneAssigned);
    } 
    
    public void SetPlayerOnPos(float oldVal, float newVal)
    {
        // Debug.Log("Health: " + newVal);
        if (health.Value<=0 && GameController.instance.zoneControllers.Count > 0 && stateMachineController.currentState.stateName!="Dead" && !GameController.instance.mapLogic.Value.isBattleRoyale){
            
            CallServerOnDeadServerRpc((int)zoneAsigned.Value);
            OnPlayerDead?.Invoke();
        }
    }

    [ClientRpc]
    public void TakeDamageClientRpc(int myDamage, ulong playerClientID, string name)
    {

        if (IsOwner)
        {
            instigatorName = name;
            if (!GameController.instance.started.Value)
            {
                return;
            }
            if ((stateMachineController.currentState.stateName == "Dead" && GameController.instance.mapLogic.Value.isBattleRoyale) || (health.Value-myDamage<=0 && GameController.instance.mapLogic.Value.isBattleRoyale))
            {
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
       
                Debug.Log("Dead");
                if (GameManager.Instance.isOnTutorial)
                {
                    Disconnect();
                    return;
                }
                playerComponentsHandler.setViewer(clientIdInstigator.Value);
                playerController.DeactivatePlayer();
                playerController.NotifyPlayerDeactivatedServerRpc(OwnerClientId, false);
                CallServerOnDeadServerRpc((int)zoneAsigned.Value);
                GameController.instance.PlayerDeadForeverServerRpc();
                stateMachineController.SetState("Viewer");
                return;
            }

            if (stateMachineController.currentState.stateName == "Dead" || health.Value<=0 || stateMachineController.currentState.stateName == "Viewer" )
            {
                return;
            }

            
            deadPosition = transform.position;
            if (IsServer)
            {
                clientIdInstigator.Value = playerClientID;
                health.Value -= (myDamage);
                StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                playerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity ,IsServer);
                playerSoundController.PlayActionSound(playerSoundController.damageTakeSound);
            }
            else
            {
                SetClientIdInstigatorServerRpc(playerClientID) ;
                SetHealthServerRpc(health.Value - (myDamage));  
                StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                playerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity , IsServer);
                playerSoundController.PlayActionSound(playerSoundController.damageTakeSound);
            }
            OnStatsChanged?.Invoke();
        
        }

    }
    [ClientRpc]
    public void SpawnDamageTakenVFXClientRpc(Vector3 pos, Quaternion rot, ulong ownerID,ClientRpcParams clientRpcParams = default)
    {
            playerVFXController.BodyDamageVFX();
    }

    [ServerRpc]
    public void SetClientIdInstigatorServerRpc(ulong clientId)
    {
        clientIdInstigator.Value = clientId;
    }
    [ClientRpc]
    public void AddAvailablePointsOnDeadClientRpc(int points, ClientRpcParams clientRpcParams= default)
    {
        if (IsServer)
        {
            avaliblePoints.Value += points;
        }
        else
        {
            avaliblePoints.Value += points;
        }
    }

    public void AddValueFromButton(int index)
    {
        statHolder[index]++;
    }
    public void SustractValueFromButton(int index)
    {
        statHolder[index]--;
    }

    public float GetSpeed()
    {
        return speed.Value;
    }
    public float GetDamageDone()
    {
        return damage.Value;
    }
    
    public int GetLevel()
    {
        return playerLevel.Value;
    }
    public float GetHealth()
    {
        return health.Value;
    }
    public int GetArmor()
    {
        return armor.Value;
    }
    public int GetHaste()
    {
        return haste.Value;
    }
    public int GetStamina()
    {
        return stamina.Value;
    }
    public int GetAvaliblePoints()
    {
        return avaliblePoints.Value;
    }
    public int GetMaxHealth()
    {
        return maxHealth.Value;
    }
    public void LevelUp()
    {
        SetLevelServerRpc(playerLevel.Value + 1);
        SetHealthServerRpc(health.Value+6);
        SetMaxHealthServerRpc(maxHealth.Value+1);
        
        CanvasController.OnUpdateUI?.Invoke();
    }

    public void AddAvaliblePoint(int value)
    {
        if (IsServer)
        {
            avaliblePoints.Value+=value;
        }
        else
        {
            addAvaliblePointsServerRpc(value);
        }
    }
    public void RemoveAvaliblePoint()
    {
        avaliblePoints.Value--;
    }

    private void OnApplicationQuit()
    {
        if (IsServer)
        {
            NetworkingHandling.HostManager.instance.DisconnectHost();
        }
        else
        {
            NetworkingHandling.ClientManager.instance.DisconnectClient(OwnerClientId);
        }
    }


    #region ServerRpc

    //template selected
    [ServerRpc]
    private void SetTemplaterServerRpc(int index)
    {
        statsTemplateSelected.Value = index;
    }
    //Stats


    [ServerRpc (RequireOwnership = false)]
    public void SetHealthServerRpc(float healthPoint)
    {
         health.Value = healthPoint;
    }
    [ServerRpc]
    public void SetHasteServerRpc(int hastePoint)
    {
        haste.Value = hastePoint;
    }
    [ServerRpc]
    public void SetNameServerRpc(FixedString32Bytes name)
    {
        userName.Value = name;
    }
    [ServerRpc]
    public void SetIsInvulnerableServerRpc(bool isDead)
    {
        this.isInvulnerable.Value = isDead;
    }
    
    [ServerRpc]
    public void SetMaxHealthServerRpc(int maxHealth)
    {
        this.maxHealth.Value = maxHealth;

    }
    [ServerRpc]
    public void SetArmorServerRpc(int armorPoint)
    {
        armor.Value = armorPoint;
    }
    [ServerRpc]
    public void SetDamageServerRpc(float damagePoint)
    {
        damage.Value = damagePoint;
    }
    [ServerRpc]
    public void SetStaminaServerRpc(int staminaPoint)
    {
        stamina.Value = staminaPoint;
    }
    [ServerRpc]
    public void SetSpeedServerRpc(float speedPoint)
    {
        speed.Value = speedPoint;

    }

    

    //Level--------
    [ServerRpc]
    public void SetLevelServerRpc(int val)
    {
        playerLevel.Value= val;
    }
    //AvaliblePoints
    [ServerRpc(RequireOwnership = false)]
    public void SetAvaliblePointsServerRpc(int val)
    {
        avaliblePoints.Value=val;
    }
    [ServerRpc(RequireOwnership = false)]
    public void addAvaliblePointsServerRpc(int val)
    {
        avaliblePoints.Value+=val;
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyKillServerRpc(bool val)
    {
        enemyKilled.Value = val;
    }

    

    [ServerRpc]
    public void SetZoneAsignedStateServerRpc(zoneColors zone)
    {
        if (IsServer)
        {
            zoneAsigned.Value = zone;
        }
        else
        {
            SetZoneAsignedClientServerRpc(zone);
        }
    }

    [ServerRpc]
    public void SetZoneAsignedClientServerRpc(zoneColors zone)
    {
        zoneAsigned.Value = zone;
    }
    #endregion

    private void OnTriggerExit(Collider other)
    {
        if (IsOwner)
        {
            if (other.CompareTag("Zone"))
            {
                isPlayerInsideTheZone = false;
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner)
        {
            if (other.CompareTag("Zone"))
            {
                isPlayerInsideTheZone = true;
            }
            if (other.CompareTag("Coin"))
            {
                    OnLevelUp?.Invoke();
                    addAvaliblePointsServerRpc(1);
                    int randomPlayer = UnityEngine.Random.Range(0, GameController.instance.numberOfPlayers.Value);
                    if (GameController.instance.players.Count>1)
                    { 
                        coinPosition.transform.position = GameController.instance.players[randomPlayer].GetComponent<PlayerStatsController>().playerZoneController.spawnCoinPoint.position;
                    }
                    if (GameManager.Instance.isOnTutorial)
                    {
                        TutorialStagesHandler.instance.SetTutorialStage(TutorialStage.UpgradeBuild);
                        Destroy(coinPosition.gameObject);
                        coinPosition = null;
                    }
                    playerSoundController.PlayActionSound(playerSoundController.pickAmmoSound);
                    CanvasController.OnBulletsAddedUI?.Invoke();
                    CanvasController.OnUpdateUI?.Invoke();

            }
        }

    }
    
    //EDITOR
#if UNITY_EDITOR
    [ContextMenu("Test Level Up")]
    public void LevelUpEditor()
        
    {
        SetLevelServerRpc(playerLevel.Value + 1);
        SetHealthServerRpc(health.Value+6);
        SetMaxHealthServerRpc(maxHealth.Value+1);
        CanvasController.OnUpdateUI?.Invoke();
    }
    [ContextMenu("Test Level Up (take damage)")]
    public void RecieveDamage()
    {
        TakeDamage(5);
        CanvasController.OnUpdateUI?.Invoke();

    }
#endif

    public void CleanData()
    {
        OnSpawnPlayer -= InitializateStats;
        if (IsClient && !IsServer )
        {
            GameManager.Instance.LoadMenuScene();
            playerComponentsHandler.SetCursorState(CursorLockMode.None, visible: true);
        }
    }

    public void OnSpawn()
    {
        
    }

}

