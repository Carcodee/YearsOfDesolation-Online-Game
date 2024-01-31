using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Players.PlayerStates;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatsController : NetworkBehaviour, IDamageable
{
    public UnityAction OnSpawnPlayer;
    public UnityAction OnStatsChanged;
    public Action OnLevelUp;    
    public Action OnPlayerDead;
    
    [Header("References")]
    public StatsTemplate[] statsTemplates;
    public NetworkVariable <int> statsTemplateSelected;
    public PlayerComponentsHandler playerComponentsHandler;
    public StateMachineController stateMachineController;
    public Transform takeDamagePosition;
    [Header("Stats")]
    [SerializeField] private NetworkVariable<int> haste = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<int> maxHealth = new NetworkVariable<int>();

    [SerializeField] private NetworkVariable<int> stamina = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<int> damage = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<int> armor = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<int> speed = new NetworkVariable<int>();

    [SerializeField] private NetworkVariable<int> playerLevel = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<int> avaliblePoints = new NetworkVariable<int>();
    
    public NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>();

    public int totalAmmo;
    public int totalBullets;
    public int currentBullets; 
    public string[] statHolderNames;
    public int[] statHolder;
    public bool isPlayerInsideTheZone;
    
    [Header("Current Gamelogic")]
    public NetworkVariable<zoneColors> zoneAsigned=new NetworkVariable<zoneColors>();
    public Transform coinPosition;
    public PlayerZoneController playerZoneController;
    public float currentOutsideTimerTick;
    public float outsideTimerTick=1;
    private bool _IsInitizalized=false;
    private bool _isInNetwork=false;

    [Header("Interfaces")]
    private IDamageable iDamageable;

    [Header("NetCode")]
    public NetworkObject playerObj;

    

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
            if (!_IsInitizalized && health.Value==0) {
                OnSpawnPlayer?.Invoke();
                _IsInitizalized = true;
            }
            OutsideZoneDamage();
            

            
        }
        //TODO: find some way to check if the player is dead with ownership

    }
    public override void OnNetworkDespawn()
    {
        OnSpawnPlayer -= InitializateStats;

    }

    public void FillStatNameHolder()
    {
        statHolderNames = new string[6];
        statHolderNames[0] =nameof (haste);
        statHolderNames[1] = nameof(health);
        statHolderNames[2] = nameof(stamina);
        statHolderNames[3] = nameof(damage);
        statHolderNames[4] = nameof(armor);
        statHolderNames[5] = nameof(speed);


    }
    public void FillArrayHolder()
    {
        statHolder= new int[6];
        statHolder[0]=haste.Value;
        statHolder[1] = health.Value;
        statHolder[2] = stamina.Value;
        statHolder[3]= damage.Value;
        statHolder[4] = armor.Value;
        statHolder[5]= speed.Value;
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
        SetDamageServerRpc(statsTemplates[statsTemplateSelected.Value].damage);
        SetArmorServerRpc(statsTemplates[statsTemplateSelected.Value].armor);
        SetSpeedServerRpc(statsTemplates[statsTemplateSelected.Value].speed);
        
        
        
        SetMaxHealthServerRpc(10);
        

        SetLevelServerRpc(1);
        SetAvaliblePointsServerRpc(3);
        health.OnValueChanged += SetPlayerOnPos;
        currentBullets=totalBullets;
        //Stats on controller player
        transform.GetComponent<PlayerController>().SetSpeedStateServerRpc(statsTemplates[statsTemplateSelected.Value].speed);
        OnStatsChanged?.Invoke();
    }

    public void SetStats()
    {
        if (IsOwner)
        {
            InitializateStats();
        }
    }

    public void RefillAmmo()
    {
        totalAmmo +=60;
    }

    public void SetHealth(int value)
    {
        if (IsServer)
        {
            health.Value = value;
        }
        else
        {
            if (IsOwner)
            {
                SetHealthServerRpc(value);
            }
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

    public void TakeDamage(int damage)
    {
        

        if (IsOwner)
        {
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
                OnStatsChanged?.Invoke();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int myDamage)
    {
        TakeDamageClientRpc(myDamage);
    }

    [ServerRpc]
    public void CallServerOnDeadServerRpc(int zoneAssigned)
    {
        GameController.instance.OnPlayerDead(zoneAssigned);
    } 
    
    public void SetPlayerOnPos(int oldVal, int newVal)
    {
        if (health.Value<=0 && GameController.instance.zoneControllers.Count > 0 && stateMachineController.currentState.stateName!="Dead"){
            OnPlayerDead?.Invoke();
            CallServerOnDeadServerRpc((int)zoneAsigned.Value);
        }
    }
    [ClientRpc]
    public void TakeDamageClientRpc(int myDamage)
    {

        if (IsOwner)
        {
            if (stateMachineController.currentState.stateName == "Dead" || health.Value<=0)
            {
                return;
            }
            
            else
            {
                if (IsServer)
                {
                    //this is wrong stat holder is controlling the health
                    health.Value -= (myDamage);
                    StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                    PlayerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity ,IsServer);
                }
                else
                {
                    SetHealthServerRpc(health.Value - (myDamage));  
                    StartCoroutine(playerComponentsHandler.ShakeCamera(0.3f, 5, 5));
                    PlayerVFXController.bloodEffectHandle.CreateVFX(takeDamagePosition.position, Quaternion.identity , IsServer);

                }
                OnStatsChanged?.Invoke();
            }
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
    public int GetDamageDone()
    {
        return damage.Value;
    }
    
    public int GetLevel()
    {
        return playerLevel.Value;
    }
    public int GetHealth()
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
        playerLevel.Value++;
    }
    public void AddAvaliblePoint()
    {
        avaliblePoints.Value++;
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
    [ServerRpc]
    public void SetHealthServerRpc(int healthPoint)
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
    public void SetDamageServerRpc(int damagePoint)
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
    [ServerRpc]
    public void SetAvaliblePointsServerRpc(int val)
    {
        avaliblePoints.Value=val;
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
        }

    }
}
