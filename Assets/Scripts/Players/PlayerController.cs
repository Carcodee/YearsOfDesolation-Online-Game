using System;
using System.Collections;
using Players.PlayerStates;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour
{
    [Header("Player Stats")]
    public PlayerStatsController playerStats;
    
    // public Action <MyVfxType, Vector3 > OnPlayerVfxAction;
    
    [Header("Player Components")]
    public GameObject cameraPrefab;
    public BulletController bulletPrefab;
    public Transform cinemachineCameraTarget;
    public CharacterController characterController;
    public PlayerComponentsHandler playerComponentsHandler;
    // public Collider [] ragdollColliders;
    // public Rigidbody[] ragdollRigidbodies;
    
    //TODO : Refactor this cam thing
    public Camera cam;
    [SerializeField] private Transform body;
    [SerializeField] private Camera cameraRef;
    public Camera topViewCamera;
    public StateMachineController stateMachineController;

    [Header("TargetConfigs")]
    public float mouseSensitivity = 100f;
    public float offset = 20.0f;
    public Transform targetPos;
    public Transform headAim;
    public Transform spawnBulletPoint;
    
    
    // [Header("Shoot")]
    // public float shootRate = 0.1f;
    // public float shootTimer = 0f;
    // public float shootRefraction = 0.1f;
    // public float currentShootRefraction = 0.1f;
    // public float minShootRefraction = 0.01f;
    

    [Header("Hit data")]
    public LayerMask playerHitLayer;
    public string [] hitTags;
    public int HeadShotDamage => (int) playerStats.GetDamageDone() * 2;
    public int legsShotDamage =>(int) playerStats.GetDamageDone() / 2;
    
    public float currentAimShootPercentage =>playerStats.currentWeaponSelected.weapon.currentShootRefraction / playerStats.currentWeaponSelected.weapon.minShootRefraction.statValue;

    [Header("Player Movement")]
    public Vector3 move;

    public float rotationFactor;
    public float rotationSmoothTime = 0.1f;
    public float rotationVelocity;
    public float slidingTime = 0.5f;
    public float slidingSpeed = 3f;
    public float sprintFactor = 2.5f;
    public float crouchFactor = 0.5f;
    public float AimingSpeedFactor = 0.5f;
    private float slidingTimer = 0f;
    public Vector3 motionSpeed;
    
    [Header("Camera Direction")]
    private int distanceFactor = 100;
    Vector3 cameraDirection;
    Vector3 groundPivot;
    public LayerMask ground;
    
    [Header("Player Actions")]
    float xRotation = 0f;
    float yRotation = 0f;

    public float reloadTime => 3/playerStats.GetHaste();
    public bool lockShoot=false;
    
    
    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private Vector3 groundPos;
    [SerializeField] private float gravityForce = 100f;
    public float gravityMultiplier = 1f;
    public Vector3 _bodyVelocity;
    public bool hasPlaned = false;

    [Header("AnimConfigs")]
    public float moveAnimationSpeed;


    [Header("GroundCheck")]
    [Range(0.1f, 50f)] public float sphereCastRadius;
    [Range(0.1f, 100f)] public float range;
    public LayerMask GroundLayer;
    public bool isGrounded;
    public Vector3 sphereOffset;
    
    
    void Start()
    {
        cam= GetComponentInChildren<Camera>();
        cam.enabled = IsOwner;
        topViewCamera.enabled = IsOwner;
        if (!cam)
        {
            Destroy(cam);
            Destroy(topViewCamera);
        }
        if (IsOwner)
        {
            // ak_99 = new Weapon(new AmmoBehaviour(100, 30, 30, 3),"Ak", 0.1f,
            //     0, 0.1f, 0.1f, 0.01f);
            WeaponItem currentWeapon= playerStats.currentWeaponSelected;
            SetSpeedStateServerRpc(5);
            stateMachineController= GetComponent<StateMachineController>();
            stateMachineController.Initializate();
            playerStats = GetComponent<PlayerStatsController>();
            playerComponentsHandler = GetComponent<PlayerComponentsHandler>();
            playerStats.OnPlayerDead += PlayerDeadCallback;
            // DoRagdoll(false);
            playerStats.currentWeaponSelected.weapon.shootRefraction = 0.1f;

        }

    }

    void Update()
    {

        if (IsOwner)
        {

            
            isGroundedCheck();
            //be care
            Reloading();

            if (playerComponentsHandler.IsPlayerLocked())
            {
                stateMachineController.SetState("Movement");
                move = Vector3.zero;
                return;
            }
            Shoot();

            stateMachineController.StateUpdate();
            if (Input.GetKey(KeyCode.Mouse1))
            {
                this.stateMachineController.networkAnimator.Animator.SetFloat("X", this.move.x);
                this.stateMachineController.networkAnimator.Animator.SetFloat("Y", this.move.z);
                //this.playerRef.AimAinimation(ref aimAnimation,networkAnimator);
                stateMachineController.networkAnimator.Animator.SetFloat("Aiming", 1);
            }
            else
            {
                stateMachineController.networkAnimator.Animator.SetFloat("Aiming", 0);
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                playerStats.currentWeaponSelected.weapon.shootRefraction = 0.01f;
            }

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                playerStats.currentWeaponSelected.weapon.shootRefraction = 0.1f; 
            }
        }
}
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            CreateAimTargetPos();

            isGroundedCheck();
            stateMachineController.StatePhysicsUpdate();

        }

    }
    private void LateUpdate()
    {
        if (IsOwner)
        {

            isGroundedCheck();
            ApplyMovement(move);
            stateMachineController.StateLateUpdate();

        }

    }

    public void isGroundedCheck()
    {

        if (Physics.SphereCast(transform.position + sphereOffset, sphereCastRadius, Vector3.down, out RaycastHit hit, range, GroundLayer))
        {
            isGrounded = true;
        }else
        {
            isGrounded = false;
        }

    }
    public void DeactivatePlayer()
    {

            characterController.enabled = false;
            // DoRagdoll(false);
            body.gameObject.SetActive(false);


    }
    public void ActivatePlayer()
    {
            characterController.enabled = true;
            body.gameObject.SetActive(true);
            // DoRagdoll(false);

    }
    public void PlayerDeadCallback()
    {
        DeactivatePlayer();
        stateMachineController.SetState("Dead");
    }
    
    public void ApplyMovement(Vector3 movement)
    {
        motionSpeed= movement * playerStats.GetSpeed() * sprintFactor * Time.deltaTime;
        motionSpeed=transform.rotation * motionSpeed;
        characterController.Move(motionSpeed );

    }
    


    public void Move(float x, float y)
    {
        move = new Vector3(x, 0, y);
    }


    public void Jump()
    {
        
        _bodyVelocity.y = Mathf.Sqrt(2* (gravityForce * gravityMultiplier) * jumpHeight);
        isGrounded = false;
    }


    public void ApplyGravity()
    {
        
        _bodyVelocity.y -= (gravityForce *gravityMultiplier) * Time.fixedDeltaTime;
        characterController.Move(_bodyVelocity* Time.fixedDeltaTime);
        
    }

    public void RotatePlayer()
    {
        Vector3 playerMovement = new Vector3(move.x, 0, move.z).normalized;

        if (playerMovement.z < 0)
        {
            return;
        }
        float targetAngle = (Mathf.Atan2(0, playerMovement.z) * Mathf.Rad2Deg) + cinemachineCameraTarget.rotation.eulerAngles.y;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, rotation, 0f);
    }

    public Vector3 GetGroundPosFromPoint(Vector3 pos)
    {
        
        Ray ray = new Ray(pos, -transform.up);
        Debug.DrawRay(pos, -transform.up * 100, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, ground))
        {
            return hit.point;
        }
        else
        {

            return transform.position;
        }
    }

    public void Reloading()
    {
        playerStats.currentWeaponSelected.weapon.Reload();

    }
    public void AimAinimation(ref float aimAnimation, NetworkAnimator networkAnimator)
    {
        if (lockShoot)
        {
            return;
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            aimAnimation += Time.deltaTime * 5;
        }
        else
        {
            aimAnimation -= Time.deltaTime * 5;
        }
        aimAnimation = Mathf.Clamp(aimAnimation, 0, 1);
        float LerpedAnim = Mathf.Clamp(Mathf.Lerp(0, 1, aimAnimation), 0, 1);
        //TODO : fix this aiming thing
        networkAnimator.Animator.SetFloat("Aiming", 1);

    }
    HitType CheckTags(string tag)
    {
        for (int i = 0; i < hitTags.Length; i++)
        {
            if (hitTags[i] == tag)
            {
                // Enum.TryParse(hitTags[i], out HitType hitType);

                return (HitType)i;
            }
        }
        
        Debug.Log("No tag found");
        return  (HitType)1;
    }
    
    float CheckDamageTags(string tag)
    {
        
        HitType hitType = CheckTags(tag);
        switch (hitType)
        {
            case HitType.Head:
                return HeadShotDamage;
            case HitType.Chest:
                return playerStats.GetDamageDone();
            case HitType.BodyR:
                return playerStats.GetDamageDone();
            case HitType.BodyL:
                return playerStats.GetDamageDone();
            case HitType.Legs:
                return legsShotDamage;
            default:
                return playerStats.GetDamageDone();
        }

    }
    // IEnumerator ShootAnimation()
    // {
    //     float currentValue = stateMachineController.networkAnimator.Animator.GetFloat("Aiming");
    //     float timeAnim = 2;
    //     while (timeAnim > 1)
    //     {
    //         stateMachineController.networkAnimator.Animator.SetFloat("Aiming", timeAnim);
    //         timeAnim -= Time.deltaTime;
    //         yield return null;
    //     }
    //     stateMachineController.networkAnimator.Animator.SetFloat("Aiming", 1);
    // }
    public void Shoot()
    {
        if (playerStats.currentWeaponSelected.weapon.ammoBehaviour.isReloading || lockShoot) return;
        
        Vector3 direction = Vector3.zero;
        float randomRefraction =Random.Range(-playerStats.currentWeaponSelected.weapon.shootRefraction , playerStats.currentWeaponSelected.weapon.shootRefraction);
        playerStats.currentWeaponSelected.weapon.shootTimer += Time.deltaTime;
        if (Input.GetKey(KeyCode.Mouse0) && playerStats.currentWeaponSelected.weapon.shootTimer > playerStats.currentWeaponSelected.weapon.shootRate.statValue && 
            playerStats.currentWeaponSelected.ammoBehaviour.currentBullets > 0 && !playerStats.currentWeaponSelected.ammoBehaviour.isReloading)
        {
            stateMachineController.networkAnimator.Animator.Play("Shoot", 1);
            StartCoroutine(playerStats.playerComponentsHandler.ShakeCamera(0.1f, .9f, .7f));
            playerStats.currentWeaponSelected.ammoBehaviour.currentBullets--;
            // OnPlayerVfxAction?.Invoke(MyVfxType.shoot ,spawnBulletPoint.position);
            PlayerVFXController.shootEffectHandle.CreateVFX(spawnBulletPoint.position, transform.rotation ,IsServer);
            playerStats.currentWeaponSelected.weapon.shootTimer = 0;
            Vector3 shotDirection = new Vector3(cameraRef.transform.forward.x + randomRefraction, cameraRef.transform.forward.y + randomRefraction, cameraRef.transform.forward.z);
            
            if (Physics.Raycast(cameraRef.transform.position, shotDirection, out RaycastHit hit, 
                    distanceFactor, playerHitLayer))
            {
                
                // OnPlayerVfxAction?.Invoke(MyVfxType.hit ,hit.point);
                PlayerVFXController.hitEffectHandle.CreateVFX(hit.point, transform.rotation ,IsServer);
                
                
                BulletController bullet = Instantiate(bulletPrefab, spawnBulletPoint.position,
                    cinemachineCameraTarget.rotation);
                bullet.Direction =  (spawnBulletPoint.transform.position - hit.point).normalized;
                bullet.damage.Value = playerStats.GetDamageDone();

                // if (hit.collider.includeLayers==playerHitLayer )
                // {
                    PlayerStatsController objectRef= hit.collider.gameObject.GetComponentInParent<PlayerStatsController>();
                    if (objectRef!=null)
                    {
                 
                        if (objectRef)
                        {
                            HitData hitData =  DamageReceiverManager.instance.CheckHitType(hit.collider.gameObject.layer);
                            // playerHitLayer
                            float damageToDo=Mathf.Floor((float)playerStats.GetDamageDone() * hitData.damageAmplifier); ;
                            Debug.Log("Damage to do: " + damageToDo);
                            Debug.Log("Layer Tag: " + hit.collider.includeLayers);
                            Debug.Log("Gameobject Name: " + hit.collider.gameObject.name);
                    
                    
                            if (IsServer)
                            {
                                objectRef.TakeDamageClientRpc((int)damageToDo);
                                CrosshairCreator.OnHitDetected?.Invoke(hitData.hitType);

                            }
                            if (IsClient)
                            {
                                objectRef.TakeDamageServerRpc((int)damageToDo);
                                CrosshairCreator.OnHitDetected?.Invoke(hitData.hitType);

                            }
                        }
                    }
                // }

            }
            else
            {
                PlayerVFXController.hitEffectHandle.CreateVFX(hit.point, transform.rotation,IsServer);
                
                BulletController bullet = Instantiate(bulletPrefab, spawnBulletPoint.position,
                    cinemachineCameraTarget.rotation);

                bullet.Direction =  (spawnBulletPoint.transform.position - hit.point).normalized;
                bullet.damage.Value = playerStats.GetDamageDone();
                // OnPlayerVfxAction?.Invoke(MyVfxType.hit ,hit.point);
                
            }
            // if (IsServer)
            // {


            playerStats.currentWeaponSelected.weapon.currentShootRefraction = playerStats.currentWeaponSelected.weapon.shootRefraction + Random.Range(0 , playerStats.currentWeaponSelected.weapon.shootRefraction);
        }
        else if(!(Input.GetKey(KeyCode.Mouse0) && playerStats.currentWeaponSelected.weapon.shootTimer > playerStats.currentWeaponSelected.weapon.shootRate.statValue && playerStats.currentWeaponSelected.ammoBehaviour.currentBullets > 0 && !playerStats.currentWeaponSelected.ammoBehaviour.isReloading))
        {
            playerStats.currentWeaponSelected.weapon.currentShootRefraction = playerStats.currentWeaponSelected.weapon.shootRefraction;
        }


        
        
    }



    public void CreateAimTargetPos()
    {

        if (Physics.Raycast(cameraRef.transform.position, cameraRef.transform.forward, out RaycastHit hit, distanceFactor, 1))
        {
            targetPos.position = hit.point;
            headAim.position = hit.point;
        }
        else
        {

            cameraDirection = cameraRef.transform.forward * distanceFactor;
            targetPos.position = cameraDirection;
            headAim.position = cameraDirection;

        }


    }
    public void SetWeapon(Weapon weapon)
    {
        // currentWeapon = weapon;
    }
    
    // public void DoRagdoll(bool value)
    // {
    //     for (int i = 0; i < ragdollRigidbodies.Length; i++)
    //     {
    //         ragdollColliders[i].enabled = value;
    //         ragdollRigidbodies[i].isKinematic = !value;
    //         ragdollRigidbodies[i].useGravity = value;
    //     }
    //     stateMachineController.networkAnimator.Animator.enabled = !value;
    // }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + sphereOffset, sphereCastRadius);
    }

    #region ServerRpc

    
    [ServerRpc]
    public void ActivateOrDeactivatePlayerServerRpc(bool value)
    {
        cam.enabled = value;
        characterController.enabled = value;
        body.gameObject.SetActive(value);
        
    }
    
    [ServerRpc]
    public void ShootServerRpc(Vector3 dir, int damage, float randomRefraction, ulong clientID)
    {
            BulletController bullet = Instantiate(bulletPrefab, spawnBulletPoint.position, cinemachineCameraTarget.rotation);
            bullet.Direction = dir.normalized + new Vector3(randomRefraction, randomRefraction, 0);
            bullet.damage.Value = damage;
            // bullet.meshRenderer.enabled = false;
            Physics.IgnoreCollision(bullet.GetComponent<Collider>(),NetworkManager.Singleton.SpawnManager.SpawnedObjects[clientID].GetComponent<Collider>());
            Physics.IgnoreCollision(bullet.GetComponent<Collider>(), bullet.GetComponent<Collider>());
            bullet.GetComponent<NetworkObject>().SpawnWithOwnership(bullet.GetComponent<NetworkObject>().OwnerClientId, true);
    }

    [ClientRpc]
    public void SpawnFakeBulletClientRpc(Vector3 dir, int damage, float randomRefraction)
    {
        if (!IsOwner)
        {
            BulletController bullet = Instantiate(bulletPrefab, spawnBulletPoint.position, cinemachineCameraTarget.rotation);
            bullet.Direction = dir.normalized + new Vector3(randomRefraction, randomRefraction, 0);
            bullet.damage.Value = damage;
        }

    }
    [ServerRpc]
    public void SetSpeedStateServerRpc(float speed)
    {

        if (IsServer)
        {
        }
        else
        {
            SetSpeedClientServerRpc(speed);
        }
    }



    [ServerRpc]
    void SetSpeedClientServerRpc(float speed)
    {
    }

    [ServerRpc]
    void SetSprintFactorServerRpc(float sprintFactor)
    {
    }

    #endregion

    #region ClientRpc

    [ClientRpc]
    void SetMainCameraClientRpc(ulong networkID)
    {
        if (IsOwner)
        {
            NetworkManager.SpawnManager.SpawnedObjects[networkID].GetComponentInChildren<BulletController>().mainCam = cam;
        }
    }
    #endregion
}

[System.Serializable]
public class WeaponItem
{
    public Weapon weapon;
    public AmmoBehaviour ammoBehaviour;

    public WeaponItem (WeaponTemplate weaponTemplate)
    {
        this.ammoBehaviour = new AmmoBehaviour(weaponTemplate.ammoType);
        this.weapon = new Weapon(ammoBehaviour, weaponTemplate);
    }
    
}

[System.Serializable]
public class Weapon
{
    public WeaponAnimations changeWeaponAnimation;
    public Sprite weaponImage;
    public AmmoBehaviour ammoBehaviour;
    public float weaponDamage;
    public string weaponName;
    public float shootTimer;
    public float shootRefraction;
    public float currentShootRefraction;
    public StatTier<float> shootRate;
    public StatTier<float> minShootRefraction;

    public Weapon(AmmoBehaviour ammoBehaviour, WeaponTemplate weaponTemplate)
    {
        this.weaponDamage = weaponTemplate.weaponDamage;
        this.changeWeaponAnimation = weaponTemplate.weaponAnimationState;
        this.weaponImage = weaponTemplate.weaponImage;
        this.ammoBehaviour =ammoBehaviour;
        this.weaponName = weaponTemplate.weaponName;
        this.shootRate.statValue = weaponTemplate.shootRate;
        this.shootTimer = weaponTemplate.shootTimer;
        this.shootRefraction = weaponTemplate.shootRefraction;
        this.currentShootRefraction = weaponTemplate.currentShootRefraction;
        this.minShootRefraction.statValue = weaponTemplate.minShootRefraction;
        shootRate.upgradeType = UpgradeType.FireRate;
        minShootRefraction.upgradeType = UpgradeType.recoil;

    }
    
    public void AddAmmo(int ammo)
    {
        ammoBehaviour.AddAmmo(ammo);
    }
    public void Reload()
    {
        ammoBehaviour.Reload();
    }
}
[System.Serializable]
public class AmmoBehaviour
{
    public int totalAmmo;
    public int currentBullets;
    public bool isReloading;
    public StatTier<int> totalBullets;
    public StatTier<float> reloadTime;
    public float reloadCurrentTime;
    public AmmoBehaviour(AmmoTypeTemplate typeTemplate)
    {
        this.totalBullets.statValue =typeTemplate.totalBullets;
        this.totalAmmo = typeTemplate.totalAmmo;
        this.currentBullets = typeTemplate.currentBullets;
        this.isReloading = false;
        this.reloadTime.statValue = typeTemplate.reloadTime;
        this.reloadCurrentTime = 0;
        totalBullets.upgradeType = UpgradeType.ClipSize;
        reloadTime.upgradeType = UpgradeType.ReloadSpeed;
        
    }
    public void AddAmmo(int ammo)
    {
        totalAmmo += ammo;
    }
    public void Reload()
    {
        if (totalAmmo <= 0)
        {
            totalAmmo = 0;
            Debug.Log("Out of ammo find coins to fill your bullets");
            return;
        }
        if (isReloading && reloadCurrentTime < reloadTime.statValue)
        {
            reloadCurrentTime += Time.deltaTime;
            if (reloadCurrentTime > reloadTime.statValue)
            {
                reloadCurrentTime = 0;
                if (totalAmmo <= totalBullets.statValue)
                {
                    int tempBulletsToFill = totalBullets.statValue - currentBullets;
                    currentBullets += totalAmmo;
                    totalAmmo -= tempBulletsToFill;
                    isReloading = false;
                }
                else
                {
                    totalAmmo -= totalBullets.statValue - currentBullets;
                    currentBullets += totalBullets.statValue - currentBullets;

                    isReloading = false;

                }
                currentBullets = Mathf.Clamp(currentBullets, 0,totalBullets.statValue);

            }
            return;
        }
        if ((Input.GetKeyDown(KeyCode.R) || currentBullets <= 0) && (currentBullets !=totalBullets.statValue))
        {
            isReloading = true;
            Debug.Log("Reloading");
        }

    }
}
public enum WeaponType
{
    Ak99=0,
    Pistol=1,
}
public enum HitType
{
    Head,
    Chest,
    BodyR,
    BodyL,
    Legs
}