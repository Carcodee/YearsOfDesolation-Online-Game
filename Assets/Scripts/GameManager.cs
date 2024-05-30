using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerController localPlayerRef;
    public bool isOnTutorial=false;

    public bool gameControllerReady = false;
    public bool gameEnded = false;
    public GameController gameControllerToSpawn;
    public NetworkManager singletonRef;
    public UnityTransport transport;
    public bool ReadyToStart = false;
    
    public GameObject loadingScene;
    public GameObject menuGameObject;
    public GameObject canvasObj;
    public string localPlayerName="";


    public float sensBeforeStart= 1.0f;
    
    public string DisconnectNotificationText = "";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    

    public async void LoadMenuScene()
    {
        AsyncOperation operation=SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        
        var loadSceneAsync = SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
        if (!loadSceneAsync.isDone)
        {
            ActivateLoadingScreen(true);
            ActivateMenu(true);
            await Task.Yield();
        }

        loadSceneAsync.allowSceneActivation = true;
        gameControllerReady = false;
        isOnTutorial = false;

        PlayerInMenuSettings();
        // if (!sceneAsync.isDone)await Task.Yield();
        // sceneAsync.allowSceneActivation = true;
    }

    public void CreateController()
    {
        GameController gameController = Instantiate(gameControllerToSpawn);
        gameController.GetComponent<NetworkObject>().Spawn();
        gameControllerReady = true;
        gameEnded = false;

    }

    public void PlayerInGameSettings()
    {
        AudioManager.instance.ActivateListener(false);
        AudioManager.instance.PlayGameSound();
    }

    public void PlayerInMenuSettings()
    {
        AudioManager.instance.ActivateListener(true);
        AudioManager.instance.PlayMenuScreenSound();
    }
    public void ActivateLoadingScreen(bool val)
    {
        if (val == true)
        {
            AudioManager.instance.PlayWaitingScreenSound();
        }
        loadingScene.gameObject.SetActive(val);
    }

    public void ActivateMenu(bool val)
    {
        canvasObj.SetActive(val);
        menuGameObject.SetActive(val);
    }


    
}
