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

    private UnityTransport _transport;
    public GameObject menu;
    public GameObject canvas;
    
    public CustomInputField lobbyName;
    [SerializeField]
    private string m_SceneName;
    
    private async void Awake()
    {
     
        _transport= FindObjectOfType<UnityTransport>();

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
        await NetworkingHandling.HostManager.instance.SetAllocation(_transport);
        await NetworkingHandling.HostManager.instance.StartHost();

    }
    

    public UnityTransport GetTransport()
    {
        return _transport;
    }

}
