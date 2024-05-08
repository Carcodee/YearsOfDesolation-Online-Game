    using System;
    using System.Collections;
using System.Collections.Generic;
    using System.Threading.Tasks;
    using Michsky.UI.ModernUIPack;
    using Unity.Netcode;
    using Unity.Services.Lobbies;
    using Unity.Services.Lobbies.Models;
    using Unity.Services.Relay;
    using Unity.Services.Relay.Models;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class MainMenuController : MonoBehaviour
    {
        public ModalWindowManager modalWindowTabs;
        public CustomDropdown customDropdown;
        public ModalWindowTabs tabs;
        public NetworkSceneManager networkSceneManager;
        public Transform lobbyList;
        public LobbyItem lobbyPrefab;
        bool isRefreshing = false;
        bool isJoining = false;
        public GameObject loadingScene;
        public bool isLobbyWindowOpen = false;
        public NotificationManager notification;
        public void OpenModalWindow()
        {
            modalWindowTabs.OpenWindow();
        }

        private void Update()
        {
            // if (isLobbyWindowOpen)
            // {
            //     var timer = HearthBeat();
            // }
            //
        }

        private void Start()
        {
            GameManager.Instance.loadingScene = loadingScene;
            GameManager.Instance.canvasObj = networkSceneManager.canvas;
            GameManager.Instance.menuGameObject = networkSceneManager.menu;
            GameManager.Instance.ReadyToStart = false;
            DisplayNotification();
        }

        public void LoadTutorial()
        {
            networkSceneManager.StartTutorialHost();
            GameManager.Instance.isOnTutorial = true;
        }

        public void DisplayNotification()
        {
            if (GameManager.Instance.DisconnectNotificationText!="")
            {
                notification.title = "Something happen";
                notification.description = GameManager.Instance.DisconnectNotificationText;
                notification.OpenNotification();
                GameManager.Instance.DisconnectNotificationText = "";
            }
        }

        public async Task HearthBeat()
        {
            await Task.Delay(15);
            
            LoadAllLobbies();
            
        }
        public async void LoadAllLobbies()
        {
            if (isRefreshing)return;
            isRefreshing = true;

            try
            {
                var options = new QueryLobbiesOptions();
                options.Count = 10;
                options.Filters = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    ),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0"
                    )
                };
                
                await DeleteTask();
                var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

      
                    for (int i = 0; i < lobbies.Results.Count; i++)
                    {
                        var item = Instantiate(lobbyPrefab,lobbyList);
                        item.Initialise(this, lobbies.Results[i]);
                        item.lobbyName.text = lobbies.Results[i].Name;
                        item.playerCount.text = lobbies.Results[i].Players.Count.ToString()+"/8 players";
                        Debug.Log("Lobby: " + lobbies.Results[i].Name);
                    }
                    
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                isRefreshing = false;

                throw;
            }
            isRefreshing = false;


        }

        public async void JoinAsync(Lobby lobby)
        {
            if (isJoining)return;
            isJoining = true;
            
            try
            {
                var joinLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                string joinCode = joinLobby.Data["JoinCode"].Value;
                GameManager.Instance.ActivateLoadingScreen(true);
                await NetworkingHandling.ClientManager.instance.StartClient(joinCode,
                    networkSceneManager.GetTransport());
                

            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
                GameManager.Instance.ActivateLoadingScreen(false);
                GameManager.Instance.ActivateMenu(true);
                isJoining = false;
                throw;
            }
            isJoining = false;
        }
        

        public async Task DeleteTask()
        {
              
            for (int i = 1; i < lobbyList.childCount; i++)
            {
                Destroy(lobbyList.GetChild(i).gameObject);
            }
               
        }
        public async Task BeginConnection(Lobby lobby)
        { 

           
        }
    }