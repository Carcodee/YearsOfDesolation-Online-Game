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
    public GameController gameControllerToSpawn;
    public NetworkManager singletonRef;
    public UnityTransport transport;
    
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
        List<AsyncOperation> operations= new List<AsyncOperation>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene currentScene = SceneManager.GetSceneAt(i);
            operations.Add(SceneManager.UnloadSceneAsync(currentScene));
        }
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        gameControllerReady = false;
        
        // if (!sceneAsync.isDone)await Task.Yield();
        // sceneAsync.allowSceneActivation = true;
    }

    public void CreateController()
    {
        GameController gameController = Instantiate(gameControllerToSpawn);
        gameController.GetComponent<NetworkObject>().Spawn();
        //game controller is trying to do things before the server is valid
        //gameController.LoadGameOptions();
        gameControllerReady = true;


    }
    public void LoadTutorialConfigs()
    {
        
    }

    public void LoadDeafaults()
    {
        
    }
    void Start()
    {
        
        

    }

    void Update()
    {
        
    }
}
