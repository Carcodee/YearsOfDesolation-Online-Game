using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    public TextMeshProUGUI lobbyName;
    public Lobby lobby;
    public TextMeshProUGUI playerCount;
    public MainMenuController menuController;
    
    public void Initialise(MainMenuController menu,Lobby lobby)
    {
        menuController = menu;
        this.lobby = lobby;
        lobbyName.text = lobby.Name;
        playerCount.text = "Players: 0 / 8";
    }
    public void Join()
    {
        menuController.JoinAsync(lobby);
    }
}
