using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Michsky.UI.ModernUIPack;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneManager : NetworkBehaviour
{
    /// INFO: You can remove the #if UNITY_EDITOR code segment and make SceneName public,
    /// but this code assures if the scene name changes you won't have to remember to
    /// manually update it. <summary>
    /// INFO: You can remove the #if UNITY_EDITOR code segment and make SceneName public,
    /// </summary>

    // public static NetworkSceneManager instance;
    public UnityTransport _transport;
    public GameObject menu;
    public GameObject canvas;
    
    public CustomInputField lobbyName;
    [SerializeField]
    private string m_SceneName;
    
    private async void Awake()
    {
       
        _transport= GameManager.Instance.transport;


        await Authenticate();

        
    }
#if UNITY_EDITOR
    public UnityEditor.SceneAsset SceneAsset;
    private void OnValidate()
    {
        if (SceneAsset != null)
        {
            m_SceneName = SceneAsset.name;
        }
    }

#endif



    public override void OnNetworkSpawn()
    {
        LoadSceneAsync();
    }

    public async void LoadSceneAsync()
    {
        if (!GameManager.Instance.gameControllerReady) await Task.Yield();
        if (IsServer && !string.IsNullOrEmpty(m_SceneName))
        {
            menu.SetActive(false);
            canvas.SetActive(false);
            var status = NetworkManager.SceneManager.LoadScene(m_SceneName, LoadSceneMode.Additive);
            if (status != SceneEventProgressStatus.Started)
            {   
                Debug.LogWarning($"Failed to load {m_SceneName} " +
                                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
    }
    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        
// #if UNITY_EDITOR
//         if (ParrelSync.ClonesManager.IsClone())
//         {
//             // When using a ParrelSync clone, switch to a different authentication profile to force the clone
//             // to sign in as a different anonymous user account.
//             string customArgument = ParrelSync.ClonesManager.GetArgument();
//             AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
//         }
// #endif
//         var options = new InitializationOptions();
//         options.SetProfile("DefaultProfile");
        if (AuthenticationService.Instance.IsSignedIn)return;
        AuthenticationService.Instance.SwitchProfile(UnityEngine.Random.Range(0, 1000000).ToString());

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    public async void StartHost()
    {
        if (lobbyName.inputText.text=="")
        {
            lobbyName.inputText.text = "My Default lobby";
        }
        NetworkingHandling.HostManager.instance.lobbyName = lobbyName.inputText.text;
        GameManager.Instance.ActivateLoadingScreen(true);
        await NetworkingHandling.HostManager.instance.SetAllocation(_transport);
        await NetworkingHandling.HostManager.instance.StartHost();

    }

    public async void StartTutorialHost()
    {
        m_SceneName = "Tutorial";
        GameManager.Instance.ActivateLoadingScreen(true);
        await NetworkingHandling.HostManager.instance.SetAllocation(_transport);
        NetworkingHandling.HostManager.instance.StartHostNoLobby();
        GameManager.Instance.CreateController();
    }
    

    public UnityTransport GetTransport()
    {
        return _transport;
    }

}
