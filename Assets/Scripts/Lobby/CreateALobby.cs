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

<<<<<<< HEAD
=======
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        if (s.name == "LobbyBrowserScene")
        {
            StartCoroutine(WaitForInputField());

            password = GameObject.Find("LobbyPasswordInputField").GetComponent<TMP_InputField>();
        }
    }
    IEnumerator WaitForInputField()
    {
        GameObject inputObj = null;

        while (inputObj == null)
        {
            inputObj = GameObject.Find("LobbyNameInputField");
            yield return null; // her frame yeniden dener
        }

        LobbyName = inputObj.GetComponent<TMP_InputField>();
        teamSize = GameObject.Find("TeamSizeDropdown").GetComponent<TMP_Dropdown>();
        lobbyPassword = GameObject.Find("PasswordInputField").GetComponent<TMP_InputField>();
        createLobbyButton = GameObject.Find("CreateLobbyButton").GetComponent<Button>();

        createLobbyButton.onClick.RemoveAllListeners();
        createLobbyButton.onClick.AddListener(CreateLobbyMethod);
    }
>>>>>>> parent of 548f87e (Kullanıcı Adı Arayüzü tasarımı gerçekleştirildi)
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
