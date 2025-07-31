using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateALobby : MonoBehaviour
{
    public TMP_InputField LobbyName;
    public TMP_Dropdown teamSize;
    public TMP_InputField lobbyPassword;
    public TMP_InputField password;

    public async void CreateLobbyMethod()
    {
        string lobbyName = LobbyName.text;
        int maxPlayers = Convert.ToInt32(teamSize.options[teamSize.value].text);
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
        {
            {"password", new DataObject(
                DataObject.VisibilityOptions.Member,
                value: lobbyPassword.text)}
        };
        options.Player = new Player(AuthenticationService.Instance.PlayerId);

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        DontDestroyOnLoad(this);
        GetComponent<CurrentLobby>().currentLobby = lobby;
        Debug.Log("Lobby created");

        StartCoroutine(HearthBeatLobbyCorootine(lobby.Id, 15f));
        SceneManager.LoadScene("LobbyRoom");
    }

    IEnumerator HearthBeatLobbyCorootine(string lobbyID, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return delay;
        }
    }
}
