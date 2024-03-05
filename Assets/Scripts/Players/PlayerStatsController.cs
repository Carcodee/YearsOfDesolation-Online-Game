using System;
using Players.PlayerStates;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public class PlayerStatsController : NetworkBehaviour, IDamageable
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
    public Transform takeDamagePosition;
    [Header("Stats")]
    [SerializeField] private NetworkVariable<int> haste = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<float> health = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<int> maxHealth = new NetworkVariable<int>();

    [SerializeField] private NetworkVariable<int> stamina = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<float> damage = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<int> armor = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<int> speed = new NetworkVariable<int>();

    [SerializeField] private NetworkVariable<int> playerLevel = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> avaliblePoints = new NetworkVariable<int>();
    
    public NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>();

    [FormerlySerializedAs("StartGameHealth")] public float startGameHealth;
    public string[] statHolderNames;
    public int[] statHolder;
    public bool isPlayerInsideTheZone;

    [Header("WeaponSpawnPoints")]
    public Transform [] weaponSpawnPoint;
    
    [Header("Current Gamelogic")]
    public NetworkVariable<zoneColors> zoneAsigned=new NetworkVariable<zoneColors>();
    public CoinBehaivor coinPosition;
    public PlayerZoneController playerZoneController;
    public float currentOutsideTimerTick;
    public float outsideTimerTick=1;
    private bool _IsInitizalized=false;
    private bool _isInNetwork=false;
    public CoinBehaivor coin;
    

    [Header("Interfaces")]
    private IDamageable iDamageable;

    [Header("NetCode")]
    public NetworkObject playerObj;
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
        }
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
            _isInNetwork = true;
            
            OnSpawnPlayer?.Invoke();
        }
    }

    private void Update()
    {
        if (IsOwner) {
            if (!_IsInitizalized && health.Value<=0) {
                OnSpawnPlayer?.Invoke();
                _IsInitizalized = true;
            }
            
            OutsideZoneDamage();

            HandleWeaponChange();

            
            if (GameController.instance.started.Value&& coinPosition==null)
            {
                SpawnPlayerCoin();
            }
        }
        //TODO: find some way to check if the player is dead with ownership

    }
    public override void OnNetworkDespawn()
    {
        OnSpawnPlayer -= InitializateStats;

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
        
    }

    void InitializateStats()
    {
        SetTemplate(0);
        playerObj = GetComponent<NetworkObject>();
        if (statsTemplates[statsTemplateSelected.Value] == null)
        {
            Debug.LogError("StatsTemplate is null");
            return;
        }
        SetHasteServerRpc(statsTemplates[statsTemplateSelected.Value].haste);
        SetHealth(10);
        Debug.Log("Health called. hpValue: " + statsTemplates[statsTemplateSelected.Value].health);

        SetStaminaServerRpc(statsTemplates[statsTemplateSelected.Value].stamina);
        SetArmorServerRpc(statsTemplates[statsTemplateSelected.Value].armor);
        SetSpeedServerRpc(statsTemplates[statsTemplateSelected.Value].speed);
        SetMaxHealthServerRpc(10);
        startGameHealth = statsTemplates[statsTemplateSelected.Value].health;

        SetLevelServerRpc(1);
        SetAvaliblePointsServerRpc(3);
        health.OnValueChanged += SetPlayerOnPos;
        ak47 = new WeaponItem(weaponsData[(int)WeaponType.Ak99]);
        doublePistols = new WeaponItem(weaponsData[(int)WeaponType.Pistol]);
        SetWeapon(ak47);
        onBagWeapon = doublePistols;
        transform.GetComponent<PlayerController>().SetSpeedStateServerRpc(statsTemplates[statsTemplateSelected.Value].speed);
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
        coinPosition= Instantiate(coin, GameController.instance.zoneControllers[coinIndexZone].transform.position, quaternion.identity);
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
            stateMachineController.SetChangingWeaponState(playerBuildSelected.first_weapon, "ChangingWeapon");
            currentWeaponSelected.weaponObjectController.gameObject.SetActive(true);
            onBagWeapon = playerBuildSelected.second_weapon;
            onBagWeapon.weaponObjectController.gameObject.SetActive(false);
            OnWeaponChanged.Invoke(currentWeaponSelected.weapon.weaponObjectController.weaponBulletSpawnPoints);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            stateMachineController.SetChangingWeaponState(playerBuildSelected.second_weapon,"ChangingWeapon");
            currentWeaponSelected.weaponObjectController.gameObject.SetActive(true);
            onBagWeapon = playerBuildSelected.first_weapon;
            onBagWeapon.weaponObjectController.gameObject.SetActive(false);
            OnWeaponChanged.Invoke(currentWeaponSelected.weapon.weaponObjectController.weaponBulletSpawnPoints);

        }

    }
    public void OutsideZoneDamage()
    {
        if (health.Value >= 0)
        {
            if (!isPlayerInsideTheZone)
            {
                currentOutsideTimerTick += Time.deltaTime;
                if (currentOutsideTimerTick > outsideTimerTick)
                {
                    currentOutsideTimerTick = 0;
                    TakeDamage(GameController.instance.mapLogic.Value.damagePerTick);
                }
            }
        }

    }

    public void TakeDamage(float damage)
    {
        if (IsOwner)
        {
            if (stateMachineController.currentState.stateName == "Dead" && GameController.instance.mapLogic.Value.isBattleRoyale)
            {
                Application.Quit();
                return;
            }
            if (stateMachineController.currentState.stateName == "Dead" || health.Value<=0)
            {
                return;
            }
            // if (health.Value<=0 && GameController.instance.zoneControllers.Count > 0 && stateMachineController.currentState.stateName!="Dead"){
            //
            //     CallServerOnDeadServerRpc((int)zoneAsigned.Value);
            //     OnPlayerDead?.Invoke();
            // }
            
            else
            {
                if (IsServer)
                {
                    //this is wrong stat holder is controlling the health
                    health.Value -= (damage);
                    StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                    PlayerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position,  Quaternion.identity,IsServer);
                        
                }
                else
                {
                    SetHealthServerRpc(health.Value - (damage));  
                    StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                    PlayerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position,  Quaternion.identity,IsServer);


                }
                CanvasController.OnUpdateUI?.Invoke();
                OnStatsChanged?.Invoke();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int myDamage, ulong clientId)
    {
        //TODO> money is being added to the player that is dead
        TakeDamageClientRpc(myDamage, clientId);
    }

    [ServerRpc]
    public void CallServerOnDeadServerRpc(int zoneAssigned)
    {
        GameController.instance.OnPlayerDead(zoneAssigned);
    } 
    
    public void SetPlayerOnPos(float oldVal, float newVal)
    {
        Debug.Log("Health: " + newVal);
        if (health.Value<=0 && GameController.instance.zoneControllers.Count > 0 && stateMachineController.currentState.stateName!="Dead"){
            
            CallServerOnDeadServerRpc((int)zoneAsigned.Value);
            OnPlayerDead?.Invoke();
        }
    }

    [ClientRpc]
    public void TakeDamageClientRpc(int myDamage, ulong playerClientID)
    {

        if (IsOwner)
        {
            if (!GameController.instance.started.Value)
            {
                return;
            }
            if (stateMachineController.currentState.stateName == "Dead" || health.Value<=0)
            {
                return;
            }
            if (IsServer)
            {
                clientIdInstigator.Value = playerClientID;
                //this is wrong stat holder is controlling the health
                health.Value -= (myDamage);
                StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                PlayerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity ,IsServer);
            }
            else
            {
                SetClientIdInstigatorServerRpc(playerClientID) ;
                SetHealthServerRpc(health.Value - (myDamage));  
                StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                PlayerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity , IsServer);

            }
            OnStatsChanged?.Invoke();
        
        }

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
    public void SetSpeedServerRpc(int speedPoint)
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
                    coinPosition.transform.position = GameController.instance.players[randomPlayer].GetComponent<PlayerStatsController>().playerZoneController.spawnCoinPoint.position;
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

}

