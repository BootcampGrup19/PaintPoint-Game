using System;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CreateALobby : MonoBehaviour
{
    public TMP_InputField LobbyName;
    public TMP_Dropdown teamSize;
    public TMP_InputField lobbyPassword;

    public async void CreateLobbyMethod()
    {
        string lobbyName = LobbyName.text;
        int maxPlayers = Convert.ToInt32(teamSize.options[teamSize.value].text);
        CreateLobbyOptions options = new CreateLobbyOptions();
        //options.Password = lobbyPassword.text;

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        DontDestroyOnLoad(this);
        Debug.Log("Lobby created");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
