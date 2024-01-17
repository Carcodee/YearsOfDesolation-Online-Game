using System.Collections;
using System.Collections.Generic;
using Players.PlayerStates;
using Unity.Netcode;
using UnityEngine;

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
    
    [Header("Cartoon")]
    public Material cartoonMat;
    public Color enemyOutlineColor;
    
    public PlayerController playerController;
    
    public SkinnedMeshRenderer skinnedMeshRenderer; 
    protected MaterialPropertyBlock mPB;
    
    


    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerController.OnPlyerShoot += ShootEffect;
        playerController.OnBulletHit += HitEffect;
        playerStatsController.OnLevelUp += LevelUpEffect;
        

        if (IsOwner)
        {

            stateMachineController = GetComponent<StateMachineController>();
            playerStatsController = GetComponent<PlayerStatsController>();

        }


    }

    public override void OnNetworkSpawn()
    {
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

    private void OnDisable()
    {
        if (IsOwner)
        {
            playerStatsController.GetComponent<PlayerController>().OnPlyerShoot -= ShootEffect;
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

    public void ApplyPointsEffect()
    {
        Instantiate(applyPointsEffectPrefabVFX, transform.position, Quaternion.identity, transform);
    }
    public void LevelUpEffect() {

        Instantiate(levelUpEffectPrefabVFX, transform.position, Quaternion.identity, transform);
    }

    public void ShootEffect()
    {
        Instantiate(ShootEffectPrefab, ShootEffectPosition.position, Quaternion.identity);
    }   
    public void HitEffect(Vector3 position)
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
