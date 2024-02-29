using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Players.PlayerStates;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerVFXController : NetworkBehaviour
{

    public StateMachineController stateMachineController;
    public PlayerStatsController playerStatsController;
    [Header("MeshTrail")]
    public float meshTrailTime= 0.5f;
    public float meshTrailTick= 0.8f;
    public float destroyTime= 0.3f;
    public bool meshTrailActive = false;
    public SkinnedMeshRenderer[] skinnedMeshRenderers;
    public Material mat;
    public string shaderVariableName;
    public float shaderVariableRate=0.1f;
    public float shaderVariableRefreshRate=0.05f;

    [Header("LevelUpGlow")]
    public Material levelUpMat;
    public float levelUpGlowTime= 0.05f;
    public float shaderVariableGlowRate = 0.1f;
    public float glowGoalValue= -7.0f;
    public string shaderVariableNameGlow;
    
    public GameObject applyPointsEffectPrefabVFX;
    public GameObject levelUpEffectPrefabVFX;

    [Header("Jump")]
    public GameObject jumpEffectPrefab;
    public Transform jumpEffectPosition;
    
    [Header("Shoot")]
    public GameObject ShootEffectPrefab;
    public GameObject hitEffectPrefab;
    public Transform ShootEffectPosition;
    
    [Header("TakeDamage")]
    public GameObject takeDamageEffectPrefab;
    
    [Header("Respawn")]
    public GameObject respawingEffectPrefab;
    public GameObject OnRespawnEffectPrefab;

    [Header("Cartoon")]
    public Material cartoonMat;
    public Color enemyOutlineColor;
    
    public PlayerController playerController;
    
    public SkinnedMeshRenderer skinnedMeshRenderer; 

    protected MaterialPropertyBlock mPB;

    [Header("HP")] 
    public Material hpMat;
    public GameObject hpObject;
    
    public Material followHpMat;
    public GameObject followHpObject;
    private bool isFollowing;
    private float followValue;
    private float followValTemp;
    private float followTime;
    private float targetVal; 
    
    [Header("VFXNetAPI")]
     public static HandleVFX shootEffectHandle;
     public static HandleVFX hitEffectHandle;
     public static HandleVFX bloodEffectHandle;
     public static HandleVFX respawningEffectHandle;
     public static HandleVFX OnRespawnEffectHandle;

     public static HandleVFX[] vfxHandles= new HandleVFX[5];

    public EmbededNetwork embededNetwork;


    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerStatsController = GetComponent<PlayerStatsController>();
        playerStatsController.OnLevelUp += LevelUpEffect;
        hpObject.SetActive(IsOwner);
        followHpObject.SetActive(IsOwner);
        if (IsOwner)
        {
            // playerController.OnBulletHit += BulletHitEffect;
            // playerController.OnPlyerShoot += ShootEffect;
             shootEffectHandle = new HandleVFX(ShootVFX, ShootEffectPrefab, HandleVFX.VfxType.Net,0);
             hitEffectHandle = new HandleVFX(HitVFX, hitEffectPrefab, HandleVFX.VfxType.Net,1);
             bloodEffectHandle= new HandleVFX(BloodVFX, takeDamageEffectPrefab, HandleVFX.VfxType.Net,2);
             respawningEffectHandle= new HandleVFX(RespawnVFX, respawingEffectPrefab, HandleVFX.VfxType.Net,3);
             OnRespawnEffectHandle= new HandleVFX(OnRespawnVFX, OnRespawnEffectPrefab, HandleVFX.VfxType.Net,4);
            vfxHandles[0] = shootEffectHandle;
            vfxHandles[1] = hitEffectHandle;
            vfxHandles[2] = bloodEffectHandle;
            vfxHandles[3] = respawningEffectHandle;
            vfxHandles[4] = OnRespawnEffectHandle;
            //
            // playerController.OnPlayerVfxAction += AddVFXOnNet;
            stateMachineController = GetComponent<StateMachineController>();
            playerStatsController = GetComponent<PlayerStatsController>();
            playerStatsController.health.OnValueChanged += UpdateHealthEffect;

        }


    }

    public override void OnNetworkSpawn()
    {
        hpObject.SetActive(IsOwner);

        if (IsOwner)
        {

            stateMachineController = GetComponent<StateMachineController>();
            playerStatsController = GetComponent<PlayerStatsController>();
            playerStatsController.OnLevelUp += LevelUpEffect;
            
        }
        else
        {
            mPB = new MaterialPropertyBlock();
            skinnedMeshRenderer.GetPropertyBlock(mPB);

            mPB.SetColor("_Outline_Color", enemyOutlineColor);
            skinnedMeshRenderer.SetPropertyBlock(mPB);

        }
    }
    public override void OnNetworkDespawn() {

        if (IsOwner) {
            // playerStatsController.GetComponent<PlayerController>().OnPlyerShoot -= ShootEffect;
            playerStatsController.OnLevelUp -= LevelUpEffect;
        } 
    }

    private void OnDisable()
    {
        if (IsOwner)
        {
            // playerStatsController.GetComponent<PlayerController>().OnPlyerShoot -= ShootEffect;
            playerStatsController.OnLevelUp -= LevelUpEffect;


        } else
        {
            
        }
    }
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Instantiate(jumpEffectPrefab, jumpEffectPosition.position, Quaternion.identity);
            }

            FollowHPBar();
        }
       
   
    }
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            if (stateMachineController.currentState.stateName == "Jetpack")
            {
                StartCoroutine(ActiveTrail(meshTrailTime));
            }
        }
        
    }

    
    public void UpdateHealthEffect(float oldVal, float newVal)
    {
        targetVal = playerStatsController.GetHealth() / playerStatsController.GetMaxHealth();
        hpMat.SetFloat("_HP",targetVal);
        isFollowing = true;
        followValTemp = followValue;

    }

    public void FollowHPBar()
    {
        if (!isFollowing)return;
        followTime += Time.deltaTime;
        followValue = Mathf.Lerp(followValTemp, targetVal, followTime);
        followHpMat.SetFloat("_HP",followValue);

        if (followTime>=1)
        {
            followTime = 0;
            isFollowing = false;

            followValue = targetVal;
            followHpMat.SetFloat("_HP",followValue);

        }

    }
    private void OnApplicationQuit()
    {
        hpMat.SetFloat("_HP", 1);
        followHpMat.SetFloat("_HP",1);

    }


    public static HandleVFX GetVFXHandle(int id)
    {
        for (int i = 0; i < vfxHandles.Length; i++)
        {
            if(vfxHandles[i].GetId()==id)
                return vfxHandles[i];
        }
        return null;
    }
    public void ApplyPointsEffect()
    {
        Instantiate(applyPointsEffectPrefabVFX, transform.position, Quaternion.identity, transform);
    }
    public void LevelUpEffect() {

        Instantiate(levelUpEffectPrefabVFX, transform.position, Quaternion.identity, transform);
    }
    public void ShootVFX(Vector3 pos, Quaternion rotation)
    {
        Instantiate(ShootEffectPrefab, pos, Quaternion.identity);
    }   

    public void HitVFX(Vector3 position, Quaternion rotation)
    {
        Instantiate(hitEffectPrefab, position, Quaternion.identity);
    }
    public void BloodVFX(Vector3 position, Quaternion rotation)
    {
        Instantiate(takeDamageEffectPrefab, position, rotation);
    }
    public void RespawnVFX(Vector3 position, Quaternion rotation)
    {
        Instantiate(respawingEffectPrefab, position, rotation);
    }
    public void OnRespawnVFX(Vector3 position, Quaternion rotation)
    {
        Instantiate(OnRespawnEffectPrefab, position, rotation);
    }
    
    public void AddVFXOnNet(MyVfxType vfxType, Vector3 pos)
    {
        switch (vfxType)
        {
            case MyVfxType.shoot:
                ShootEffect(pos);
                break;
            case MyVfxType.hit:
                BulletHitEffect(pos);
                break;
            
        }
    }
    public void ShootEffect(Vector3 pos)
    {
        if (IsServer)
        {
            ShootEffectClientRpc(pos);
        }
        else
        {
            CallShootEffectServerRpc(pos);
        }
    }
    
    public void BulletHitEffect(Vector3 pos)
    {
        if (IsServer)
        {
            HitEffectClientRpc(pos);
        }
        else
        {
            CallHitEffectServerRpc(pos);
        }
    }
    


    [ServerRpc]
    public void CallShootEffectServerRpc(Vector3 pos)
    {
        ShootEffectClientRpc(pos);
    }
    
    [ServerRpc]
    public void CallHitEffectServerRpc(Vector3 position)
    {
        HitEffectClientRpc(position);
    }
    
    [ClientRpc]
    public void ShootEffectClientRpc(Vector3 pos)
    {
        Instantiate(ShootEffectPrefab, pos, Quaternion.identity);
    }   


    [ClientRpc]
    public void HitEffectClientRpc(Vector3 position)
    {
        Instantiate(hitEffectPrefab, position, Quaternion.identity);
    }
    public void AnimateGlowMaterial()
    {
        levelUpMat.GetFloat("_FresnelIntensity");
        StartCoroutine(AnimateMaterial(levelUpMat, glowGoalValue, shaderVariableGlowRate, levelUpGlowTime, shaderVariableNameGlow));
    }
    public IEnumerator ActiveTrail(float timeActive)
    {

            timeActive -= meshTrailTick;

            if (skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                Debug.Log("ActiveTrail");
                GameObject meshObj = new GameObject();
                meshObj.transform.SetPositionAndRotation(transform.position, transform.rotation);
                MeshRenderer mr= meshObj.AddComponent<MeshRenderer>();
                MeshFilter mf=meshObj.AddComponent<MeshFilter>();
                Mesh mesh= new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);
                mf.mesh=mesh;
                mr.material=mat;
                StartCoroutine(AnimateMaterial(mr.material, 0, shaderVariableRate, shaderVariableRefreshRate, shaderVariableName));
                Destroy(meshObj, destroyTime);

            }

            yield return new WaitForSeconds(meshTrailTick);
           
        
        meshTrailActive=false;
    }
    IEnumerator AnimateMaterial(Material mat, float goal, float rate, float refreshRate, string variableName)
    {
        float valueToAnim = mat.GetFloat(variableName);
        while (valueToAnim>goal)
        {
            valueToAnim -= rate;
            mat.SetFloat(shaderVariableName, valueToAnim);
            yield return new WaitForSeconds(refreshRate);
        }
    }


    
}
[System.Serializable]
public class HandleVFX 
{
    private Action <Vector3, Quaternion> _serverRpcActions;
    private Action <Vector3, Quaternion> _clientRpcActions;
    private EmbededNetwork embededNetwork;
    private int id;
    VfxType _vfxType;
    

    //create a constructor that takes an action with a Vector3 parameter
    public HandleVFX(Action<Vector3, Quaternion> actions, GameObject vfx,VfxType vfxType, int id)
    {
        this.id = id;
        embededNetwork = EmbededNetwork.Instance;
        embededNetwork.actionToCallAtPos.Add(actions);
        _clientRpcActions += actions;
        embededNetwork.actionToCallAtPos[id] = _clientRpcActions;

        _vfxType = vfxType;
    }
 
    public int GetId()
    {
        return id;
    }
    public void CreateVFX(Vector3 value, Quaternion rotation,bool isServer)
    {
        HandleActions(_vfxType, value, rotation, isServer);
    }
    public void CreateLocalVFX(Vector3 value, Quaternion rotation)
    {
     
        _clientRpcActions.Invoke(value, rotation);
    }
    
    
    public void CallClientServerRpc(Vector3 value, Quaternion rotation)
    {
        embededNetwork.CallMyCustomClient_ServerRPC(value,rotation,id);
    }
    public void ActionClientRpc(Vector3 value, Quaternion rotation)
    {
        embededNetwork.MyCustomClientRpc(value,rotation ,id);
    }
    public void GenerateVFXOnNet(Vector3 parameter, Quaternion rotation,bool isServer)
    {
        if (isServer)
        {
            // CreateLocalVFX(parameter, rotation);
            ActionClientRpc(parameter, rotation);
        }
        else
        {
            // CreateLocalVFX(parameter, rotation);
            CallClientServerRpc(parameter, rotation);
        }
    }
    
    public void HandleActions(VfxType bitFlagType, Vector3 value, Quaternion rotation,bool isServer)
    {
        VfxType checkType = bitFlagType;
        
        if ((checkType & VfxType.AtPoint) == VfxType.AtPoint)
        {
            
        }
        if ((checkType & VfxType.LocalOnly) == VfxType.LocalOnly)
        {
            CreateLocalVFX(value, rotation);
        }
        if ((checkType & VfxType.Net) == VfxType.Net)
        {
            GenerateVFXOnNet(value,rotation, isServer);

        }
        
        
    } 

    
    [Flags]
    public enum VfxType
    {
        AtPoint=0,
        LocalOnly=1 << 1,
        Net=1 << 2,
    }
    

}
public enum MyVfxType
{
    hit,
    shoot,
    jump,
    levelUp,
    applyPoints,
    trail,
    
}