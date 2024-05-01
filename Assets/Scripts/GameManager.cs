using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerController localPlayerRef;
    public bool isOnTutorial=false;

    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    public void OnDestroy()
    {
        DontDestroyOnLoad(gameObject);
    }
    public async void LoadMenuScene()
    {
        
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene currentScene = SceneManager.GetSceneAt(i);
            var unloadSceneAsync = SceneManager.UnloadSceneAsync(currentScene);
            
        }
        
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        // if (!sceneAsync.isDone)await Task.Yield();
        // sceneAsync.allowSceneActivation = true;
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
