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
    using Random = System.Random;

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

        public CustomInputField usernameInputField;
        public GameObject mainMap;
        public GameObject optionsPanel;
        public GameObject controlsPanel;
        
        
        public OptionObjectManager sensitivity;
        public OptionObjectManager gameplayVolume;
        public OptionObjectManager backGroundVolume;
        public OptionObjectManager masterVolume;
        
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
            Cursor.visible = true;
            DisplayNotification();
        }

        public void ActivateOptions(bool val)
        {
            optionsPanel.SetActive(val);
            GetSensitivity();
            GetBackgroundSound();
            GetMasterSound();
            GetGameplaySound();
        }
        
        public void LoadTutorial()
        {
            networkSceneManager.StartTutorialHost();
            GameManager.Instance.isOnTutorial = true;
            GameManager.Instance.ActivateLoadingScreen(true);
            
            mainMap.SetActive(false);
        }

        public void DisplayNotification()
        {
            if (GameManager.Instance.DisconnectNotificationText!="")
            {
                Debug.Log(GameManager.Instance.DisconnectNotificationText);
                if(GameManager.Instance.DisconnectNotificationText=="Congratulations!, You finished the tutorial")
                {
                    notification.title = "You are ready";
                    notification.OpenNotification();
                    notification.description = GameManager.Instance.DisconnectNotificationText;
                    GameManager.Instance.DisconnectNotificationText = "";
                }
                else if(GameManager.Instance.DisconnectNotificationText=="Max waiting time was surpassed")
                {
                    notification.title = "Lobby Disconnection";
                    notification.OpenNotification();
                    notification.description = GameManager.Instance.DisconnectNotificationText;
                    GameManager.Instance.DisconnectNotificationText = "";
                }
                else if(GameManager.Instance.DisconnectNotificationText=="YOU WIN")
                {
                    notification.title = "CONGRATULATIONS!";
                    notification.OpenNotification();
                    notification.description = GameManager.Instance.DisconnectNotificationText;
                    GameManager.Instance.DisconnectNotificationText = "";
                }else 
                {
                    notification.title = "Something happen";
                    notification.OpenNotification();
                    notification.description = GameManager.Instance.DisconnectNotificationText;
                    GameManager.Instance.DisconnectNotificationText = "";
                }
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
                        item.Joinable = !lobbies.Results[i].IsLocked;
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
                if (usernameInputField.inputText.text=="")
                {
                    Random newRandomVal = new Random();
                    usernameInputField.inputText.text = "RandomNPC" + newRandomVal.Next(0, 10000) ;
                }

                GameManager.Instance.localPlayerName = usernameInputField.inputText.text;

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
        
    public void SetBackgroundSound()
    {
        AudioManager.instance.SetBackgroundSound(backGroundVolume.slider.mainSlider.value);
        GameManager.Instance.backgroundSoundBeforeStart = backGroundVolume.slider.mainSlider.value;
    }
    public void SetGameplaySound()
    {
        AudioManager.instance.SetGameplaySound(gameplayVolume.slider.mainSlider.value);
        GameManager.Instance.gameplaySoundBeforeStart = gameplayVolume.slider.mainSlider.value;
    }
    public void SetMasterSound()
    {
        AudioManager.instance.SetMasterSound(masterVolume.slider.mainSlider.value);
        GameManager.Instance.masterSoundBeforeStart = masterVolume.slider.mainSlider.value;
    }
    public void SetSensitivity()
    {
        GameManager.Instance.sensBeforeStart = sensitivity.slider.mainSlider.value;
    }

    public void GetBackgroundSound()
    {
        backGroundVolume.slider.mainSlider.value = GameManager.Instance.backgroundSoundBeforeStart;
    }
    public void GetGameplaySound()
    {
        gameplayVolume.slider.mainSlider.value =  GameManager.Instance.gameplaySoundBeforeStart;
    }
    public void GetMasterSound()
    {
        masterVolume.slider.mainSlider.value = GameManager.Instance.masterSoundBeforeStart;
    }
    public void GetSensitivity()
    {
         sensitivity.slider.mainSlider.value = GameManager.Instance.sensBeforeStart;
    }

    public void OpenControls(bool val)
    {
        controlsPanel.gameObject.SetActive(val);
    }
    public void QuitApp()
    {
        Application.Quit();
    }

}