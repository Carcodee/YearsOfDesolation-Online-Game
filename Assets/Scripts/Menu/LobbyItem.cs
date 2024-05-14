using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public TextMeshProUGUI lobbyName;
    public Lobby lobby;
    public TextMeshProUGUI playerCount;
    public MainMenuController menuController;
    public bool Joinable=true;
    public TextMeshProUGUI lockedText;
    public ButtonManager button;
    public void Initialise(MainMenuController menu,Lobby lobby)
    {
        menuController = menu;
        this.lobby = lobby;
        lobbyName.text = lobby.Name;
        playerCount.text = "Players: 0 / 8";
        if (!Joinable)
        {
            button.buttonText = "IN-GAME";
            button.GetComponent<Button>().interactable = false;
            
        }
    }
    public void Join()
    {
        menuController.JoinAsync(lobby);
        
    }
}
