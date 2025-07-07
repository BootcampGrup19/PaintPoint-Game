using System;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class JoinLobby : MonoBehaviour
{
    public TMP_InputField lobbyCode;
    public TMP_InputField password;

    public async void JoinLobbyWithLobbyCode()
    {
        var code = lobbyCode.text;
        var enteredPassword = password.text;
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();

            if (!string.IsNullOrEmpty(enteredPassword))
            {
                options.Password = enteredPassword;
            }

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;

            Debug.Log("Joined lobby with code: " + code);
            SceneManager.LoadScene("LobbyRoom");
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    public async void JoinLobbyWithLobbyId(string lobbyId)
    {
        var enteredPassword = password.text;
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();

            if(!string.IsNullOrEmpty(enteredPassword))
            {
                options.Password = enteredPassword;
            }

            Lobby lobby1 = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby1;

            Debug.Log("Joined lobby with id: " + lobbyId);
            SceneManager.LoadScene("LobbyRoom");

        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    public async void QuickJoinMethod()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
            Debug.Log("Joined lobby with Quick Join: " + lobby.Id);
            Debug.Log("lobby code: " + lobby.LobbyCode);
            SceneManager.LoadScene("LobbyRoom");
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
}
