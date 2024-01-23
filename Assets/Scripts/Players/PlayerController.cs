using System;
using Players.PlayerStates;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
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
    public Collider [] ragdollColliders;
    public Rigidbody[] ragdollRigidbodies;
    
    //TODO : Refactor this cam thing
    public Camera cam;
    [SerializeField] private Transform body;
    [SerializeField] private Camera cameraRef;
    public StateMachineController stateMachineController;

    [Header("TargetConfigs")]
    public float mouseSensitivity = 100f;
    public float offset = 20.0f;
    public Transform targetPos;
    public Transform headAim;
    public Transform spawnBulletPoint;
    
    [Header("Shoot")]
    public float shootRate = 0.1f;
    public float shootTimer = 0f;
    public float shootRefraction = 0.1f;
    public float currentShootRefraction = 0.1f;
    public float minShootRefraction = 0.01f;

    public float currentAimShootPercentage => currentShootRefraction / minShootRefraction;

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

    [Header("Camera Direction")]
    private int distanceFactor = 100;
    Vector3 cameraDirection;
    Vector3 groundPivot;
    public LayerMask ground;

    [Header("Player Actions")]
    float xRotation = 0f;
    float yRotation = 0f;

    public float reloadTime => 3/playerStats.GetHaste();
    public float reloadCurrentTime = 0f;
    public bool isReloading = false;

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
        if (!cam)
        {
            Destroy(cam);
        }
        if (IsOwner)
        {
            SetSpeedStateServerRpc(5);
            stateMachineController= GetComponent<StateMachineController>();
            stateMachineController.Initializate();
            playerStats = GetComponent<PlayerStatsController>();
            playerComponentsHandler = GetComponent<PlayerComponentsHandler>();
            playerStats.OnPlayerDead += PlayerDeadCallback;
            DoRagdoll(false);
            shootRefraction = 0.1f;

        }

    }

    void Update()
    {

        if (IsOwner)
        {

            
            isGroundedCheck();
            Reloading();
            CreateAimTargetPos();

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
                shootRefraction = 0.01f;
            }

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                shootRefraction = 0.1f; 
            }
        }
}
    private void FixedUpdate()
    {
        if (IsOwner)
        {
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
            DoRagdoll(false);
            body.gameObject.SetActive(false);


    }
    public void ActivatePlayer()
    {
            characterController.enabled = true;
            body.gameObject.SetActive(true);
            DoRagdoll(false);

    }
    public void PlayerDeadCallback()
    {
        DeactivatePlayer();
        stateMachineController.SetState("Dead");
    }
    
    public void ApplyMovement(Vector3 movement)
    {
        Vector3 motion= movement * playerStats.GetSpeed() * sprintFactor * Time.deltaTime;
        motion=transform.rotation * motion;
        characterController.Move(motion );

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
        if (playerStats.totalAmmo <= 0)
        {
            playerStats.totalAmmo = 0;
            Debug.Log("Out of ammo find coins to fill your bullets");
            return;
        }
        if (isReloading && reloadCurrentTime < reloadTime)
        {
            reloadCurrentTime += Time.deltaTime;
            if (reloadCurrentTime > reloadTime)
            {
                reloadCurrentTime = 0;
                if (playerStats.totalAmmo <= playerStats.totalBullets)
                {
                    int tempBulletsToFill = playerStats.totalBullets - playerStats.currentBullets;
                    playerStats.currentBullets += playerStats.totalAmmo;
                    playerStats.totalAmmo -= tempBulletsToFill;
                    isReloading = false;
                }
                else
                {
                    playerStats.totalAmmo -= playerStats.totalBullets - playerStats.currentBullets;
                    playerStats.currentBullets += playerStats.totalBullets - playerStats.currentBullets;

                    isReloading = false;

                }
                playerStats.currentBullets = Mathf.Clamp(playerStats.currentBullets, 0, playerStats.totalBullets);

            }
            return;
        }
        if ((Input.GetKeyDown(KeyCode.R) || playerStats.currentBullets <= 0) && (playerStats.currentBullets != playerStats.totalBullets))
        {
            isReloading = true;
            Debug.Log("Reloading");
        }
    }
    public void AimAinimation(ref float aimAnimation, NetworkAnimator networkAnimator)
    {
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
    public void Shoot()
    {                
        Vector3 direction = Vector3.zero;
        float randomRefraction =Random.Range(-shootRefraction , shootRefraction);
        shootTimer += Time.deltaTime;
        if (Input.GetKey(KeyCode.Mouse0) && shootTimer > shootRate && playerStats.currentBullets > 0 && !isReloading)
        {

            StartCoroutine(playerStats.playerComponentsHandler.ShakeCamera(0.1f, .9f, .7f));
            playerStats.currentBullets--;
            // OnPlayerVfxAction?.Invoke(MyVfxType.shoot ,spawnBulletPoint.position);
            PlayerVFXController.shootEffectHandle.CreateVFX(spawnBulletPoint.position, transform.rotation ,IsServer);
            shootTimer = 0;
            Vector3 shotDirection = new Vector3(cameraRef.transform.forward.x + randomRefraction, cameraRef.transform.forward.y + randomRefraction, cameraRef.transform.forward.z);
            
            if (Physics.Raycast(cameraRef.transform.position, shotDirection, out RaycastHit hit, 
                    distanceFactor))
            {
                
                // OnPlayerVfxAction?.Invoke(MyVfxType.hit ,hit.point);
                PlayerVFXController.hitEffectHandle.CreateVFX(hit.point, transform.rotation ,IsServer);

                hit.collider.gameObject.TryGetComponent<PlayerStatsController>(out PlayerStatsController enemyRef);
                if (enemyRef)
                {
                    if (IsServer)
                    {
                        enemyRef.TakeDamageClientRpc(playerStats.GetDamageDone());
                        CrosshairCreator.OnHitDetected?.Invoke();

                    }
                    if (IsClient)
                    {
                        enemyRef.TakeDamageServerRpc(playerStats.GetDamageDone());
                        CrosshairCreator.OnHitDetected?.Invoke();

                    }
                    Debug.Log(enemyRef.name);
                }
            }
            else
            {
                PlayerVFXController.hitEffectHandle.CreateVFX(hit.point, transform.rotation,IsServer);

                // OnPlayerVfxAction?.Invoke(MyVfxType.hit ,hit.point);
                
            }
            // if (IsServer)
            // {
            BulletController bullet = Instantiate(bulletPrefab, spawnBulletPoint.position,
                cinemachineCameraTarget.rotation);
            //     Physics.IgnoreCollision(bullet.GetComponent<Collider>(),characterController.GetComponent<Collider>());
            //     Physics.IgnoreCollision(bullet.GetComponent<Collider>(),bullet.GetComponent<Collider>());
            //
                bullet.Direction = shotDirection.normalized ;
                bullet.damage.Value = playerStats.GetDamageDone();
            //     bullet.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, true);
            // }
            // else
            // {
            //     BulletController bullet = Instantiate(bulletPrefab, spawnBulletPoint.position, cinemachineCameraTarget.rotation);
            //     Physics.IgnoreCollision(bullet.GetComponent<Collider>(),characterController.GetComponent<Collider>());
            //     Physics.IgnoreCollision(bullet.GetComponent<Collider>(),bullet.GetComponent<Collider>());
            //     bullet.Direction = direction.normalized + new Vector3(randomRefraction, randomRefraction, 0);
            //     bullet.damage.Value = playerStats.GetDamageDone();
            //     // bullet.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, true);
            // }
            // SpawnFakeBulletClientRpc(direction, playerStats.GetDamageDone(),randomRefraction);
            currentShootRefraction = shootRefraction + Random.Range(0 , shootRefraction);
        }
        else if(!(Input.GetKey(KeyCode.Mouse0) && shootTimer > shootRate && playerStats.currentBullets > 0 && !isReloading))
        {
            currentShootRefraction = shootRefraction;
        }


        
        
    }



    public void CreateAimTargetPos()
    {

        if (Physics.Raycast(cameraRef.transform.position, cameraRef.transform.forward, out RaycastHit hit, distanceFactor))
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
    
    public void DoRagdoll(bool value)
    {
        for (int i = 0; i < ragdollRigidbodies.Length; i++)
        {
            ragdollColliders[i].enabled = value;
            ragdollRigidbodies[i].isKinematic = !value;
            ragdollRigidbodies[i].useGravity = value;
        }
        stateMachineController.networkAnimator.Animator.enabled = !value;
    }

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

public class AmmoBehaviour
{
    int totalAmmo;
    int currentBullets;
    int totalBullets;
    bool isReloading;
    float reloadTime;
    float reloadCurrentTime;
    public AmmoBehaviour(int totalAmmo, int currentBullets, int totalBullets, bool isReloading, float reloadTime, float reloadCurrentTime)
    {
        this.totalBullets = totalBullets;
        this.totalAmmo = totalAmmo;
        this.currentBullets = currentBullets;
        this.isReloading = isReloading;
        this.reloadTime = reloadTime;
        this.reloadCurrentTime = reloadCurrentTime;
        
        
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
        if (isReloading && reloadCurrentTime < reloadTime)
        {
            reloadCurrentTime += Time.deltaTime;
            if (reloadCurrentTime > reloadTime)
            {
                reloadCurrentTime = 0;
                if (totalAmmo <= totalBullets)
                {
                    int tempBulletsToFill = totalBullets - currentBullets;
                    currentBullets += totalAmmo;
                    totalAmmo -= tempBulletsToFill;
                    isReloading = false;
                }
                else
                {
                    totalAmmo -= totalBullets - currentBullets;
                    currentBullets += totalBullets - currentBullets;

                    isReloading = false;

                }
                currentBullets = Mathf.Clamp(currentBullets, 0,totalBullets);

            }
            return;
        }
        if ((Input.GetKeyDown(KeyCode.R) || currentBullets <= 0) && (currentBullets !=totalBullets))
        {
            isReloading = true;
            Debug.Log("Reloading");
        }

    }
}