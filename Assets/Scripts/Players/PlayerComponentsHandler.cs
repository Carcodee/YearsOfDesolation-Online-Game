using System;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Menu.StatsPanel;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerComponentsHandler : NetworkBehaviour
{
    public static Action<string> OnStationaryStepAttempted;
    public PlayerInput playerInput;

    public StarterAssetsInputs input;
    public InputActionAsset playerControls;

    [Header("Player Components")]
    public Canvas canvasPrefab;
    public Canvas UIRenderer;

    [Header("UI and Gameplay Canvases")]
    public Canvas canvasObject;
    public Canvas canvasRenderer;
    
    public GameObject cameraPrefab;
    public GameObject cameraZoomPrefab;
    public GameObject sprintCameraPrefab;
    private Rigidbody rb;
    public PlayerController playerController;

    [Header("Ref")]
    public CinemachineVirtualCamera cinemachineVirtualCameraInstance;
    public CinemachineVirtualCamera cinmachineCloseLookCameraIntance;
    public CinemachineVirtualCamera cinmachineSprintCameraIntance;
    public MinimapCameraController playerMinimapCamera;
    
    [Header("UI")]
    public TextMeshProUGUI playerNameText;
    public StatsPanelController statsPanelController;
    public PauseController pauseController;

    float timer = 0;

    [Header("Cinemachine Camera config")]
    public Transform cinemachineCameraTarget;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    public float rotationDetector;
    public float maxForRotation=10.5f;
    
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    private bool IsCurrentDeviceMouse=true;
    private const float _threshold = 0;

    

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            InstanciateComponents();

            // Attach input events
            playerInput.onActionTriggered += HandleAction;
        }

    }
    private void Awake()
    {
        if (IsOwner)
        {

        }
    }
    void Start()
    {


    }
    void Update()
    {
        if (IsOwner)
        {
            IsCurrentDeviceMouse = !statsPanelController.isPanelOpen && !pauseController.pauseMenu.activeSelf ;
            if (IsCurrentDeviceMouse)
            {
                //on game
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                TransitionCamera();
            }
            else 
            {
                //on panel
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            CameraRotation(input.look);
        }
    }


    private void HandleAction(InputAction.CallbackContext context)
    {
        if (context.action.name == "ExactNameOfYourLookAction") // Replace with your action's name
        {
            CameraRotation(input.look);
        }
        // Handle other actions as needed
    }

    public bool IsPlayerLocked()
    {
        return !IsCurrentDeviceMouse;
    }

    void InstanciateComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        input = GetComponent<StarterAssetsInputs>();
        input = transform.GetComponent<StarterAssetsInputs>();
        playerInput.enabled = true;
        if (playerInput.actions != playerControls)
        {
            playerInput.actions = playerControls;
        }

        _cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        //Normal camera
        GameObject camera = Instantiate(cameraPrefab);
        cinemachineVirtualCameraInstance = camera.GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCameraInstance.Follow = cinemachineCameraTarget;
        //zoom camera
        GameObject cameraZoom = Instantiate(cameraZoomPrefab);
        cinmachineCloseLookCameraIntance=cameraZoom.GetComponentInChildren<CinemachineVirtualCamera>();
        cinmachineCloseLookCameraIntance.Follow = cinemachineCameraTarget;
        
        GameObject sprintCamera = Instantiate(sprintCameraPrefab);
        cinmachineSprintCameraIntance = sprintCamera.GetComponentInChildren<CinemachineVirtualCamera>();
        cinmachineSprintCameraIntance.Follow = cinemachineCameraTarget;
        
        //Minimap
        playerMinimapCamera.transform.SetParent(null);
        //Canvas
        // canvasObject = Instantiate(canvasPrefab,transform);
        // canvasObject.GetComponentInChildren<Button>().onClick.AddListener(transform.GetComponent<PlayerStatsController>().OnSpawnPlayer);
        // playerNameText = canvasObject.GetComponentInChildren<TextMeshProUGUI>();
        // statsPanelController = GetComponentInChildren<StatsPanelController>();
        // pauseController = GetComponentInChildren<PauseController>();


        //rb = GetComponent<Rigidbody>();
        //rb.isKinematic = true;
    }


    public void CreateCanvas(Camera camera)
    {
        canvasObject = Instantiate(canvasPrefab,transform);
        canvasObject.worldCamera = camera;
        canvasObject.planeDistance = 0.35f;

        canvasRenderer= Instantiate(UIRenderer,transform);
        playerNameText = canvasObject.GetComponentInChildren<TextMeshProUGUI>();
        statsPanelController =canvasRenderer.GetComponentInChildren<StatsPanelController>();
        pauseController = canvasRenderer.GetComponentInChildren<PauseController>();    
        pauseController.playerAssigned = playerController;
        pauseController.playerCrosshair = canvasObject.GetComponent<CrosshairCreator>();
    }
    private void CameraRotation(Vector3 look)
    {
        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= _threshold )
        {
            //Don't multiply mouse input by Time.deltaTime;

            float deltaTimeMultiplier =(IsCurrentDeviceMouse) ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += look.y * deltaTimeMultiplier;
            rotationDetector += look.x * deltaTimeMultiplier;
        }
        // clamp our rotations so our values are limited 360 degrees

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
        if (rotationDetector<-maxForRotation)
        {
            rotationDetector = 0;
            OnStationaryStepAttempted?.Invoke("RotatingLeft");
        }else if (rotationDetector>maxForRotation)
        {
            rotationDetector = 0;
            OnStationaryStepAttempted?.Invoke("RotatingRight");
        }
        // Cinemachine will follow this target
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    private void TransitionCamera()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            cinmachineCloseLookCameraIntance.Priority = 25;
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            cinmachineCloseLookCameraIntance.Priority = 5;
        }
        if (playerController.isSprinting)
        {
            cinmachineSprintCameraIntance.Priority = 20;
        }   
        if (!playerController.isSprinting)
        {
            cinmachineSprintCameraIntance.Priority = 5;
        }
    }
    
    public IEnumerator ShakeCamera(float duration, float magnitude, float frecuency)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            MyUtilities.StartCameraShake(cinemachineVirtualCameraInstance, magnitude, frecuency, 0);
            MyUtilities.StartCameraShake(cinmachineCloseLookCameraIntance, magnitude, frecuency, 0);
            MyUtilities.StartCameraShake(cinmachineSprintCameraIntance, magnitude, frecuency, 0);

            yield return null;
        }

        MyUtilities.StopCameraShake(cinemachineVirtualCameraInstance);
        MyUtilities.StopCameraShake(cinmachineCloseLookCameraIntance);
        MyUtilities.StopCameraShake(cinmachineSprintCameraIntance);
        
    }
}
