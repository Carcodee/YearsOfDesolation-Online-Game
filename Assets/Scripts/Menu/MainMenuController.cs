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

        public void OpenModalWindow()
        {
            modalWindowTabs.OpenWindow();
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
                var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);


                
                // for (int i = 1; i < lobbyList.childCount; i++)
                // {
                //     
                //     Destroy(lobbyList.GetChild(i).gameObject);
                //     
                // }
                
                for (int i = 0; i < lobbies.Results.Count; i++)
                {
                    var item = Instantiate(lobbyPrefab,lobbyList);
                    item.Initialise(this, lobbies.Results[i]);
                    item.lobbyName.text = lobbies.Results[i].Name;
                    Debug.Log("Lobby: " + lobbies.Results[i].Name);
                }
                Debug.Log("Lobbies: " + lobbies.Results.Count);
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
                networkSceneManager.menu.SetActive(false);
                networkSceneManager.canvas.SetActive(false);
                await NetworkingHandling.ClientManager.instance.StartClient(joinCode,
                    networkSceneManager.GetTransport());

            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
                isJoining = false;
                throw;
            }
            isJoining = false;
        }
        
        public async Task BeginConnection(Lobby lobby)
        { 

           
        }
    }