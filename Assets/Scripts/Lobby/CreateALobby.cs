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
using UnityEngine.UI;

public class CreateALobby : MonoBehaviour
{
    public TMP_InputField LobbyName;
    public TMP_Dropdown teamSize;
    public TMP_InputField lobbyPassword;
    public TMP_InputField password;
    public Button createLobbyButton;

    private void Start()
    {
        /*LobbyName = GameObject.Find("LobbyNameInputField").GetComponent<TMP_InputField>();
        teamSize = GameObject.Find("TeamSizeDropdown").GetComponent<TMP_Dropdown>();
        lobbyPassword = GameObject.Find("PasswordInputField").GetComponent<TMP_InputField>();
        password = GameObject.Find("LobbyPasswordInputField").GetComponent<TMP_InputField>();
        createLobbyButton =GameObject.Find("Create Lobby").GetComponent<Button>();*/

        createLobbyButton.onClick.AddListener(CreateLobbyMethod);
    }
    public async void CreateLobbyMethod()
    {
        string lobbyName = LobbyName.text;
        int maxPlayers = Convert.ToInt32(teamSize.options[teamSize.value].text);
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
        {
            {"password", new DataObject(
                DataObject.VisibilityOptions.Member,
                value: lobbyPassword.text)},
            {"maxplayers",
                new DataObject(DataObject.VisibilityOptions.Private,
                value: Convert.ToString(maxPlayers))}
        };
        options.Player = new Player(AuthenticationService.Instance.PlayerId);

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        DontDestroyOnLoad(this);

        CurrentLobby.Instance.currentLobby = lobby;
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
