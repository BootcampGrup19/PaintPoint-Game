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

<<<<<<< HEAD
=======
    void OnEnable(){
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable(){
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        if (s.name == "LobbyBrowserScene")
        {
            lobbyCode = GameObject.Find("LobbyJoinCodeInputField").GetComponent<TMP_InputField>();
            password = GameObject.Find("LobbyPasswordInputField").GetComponent<TMP_InputField>();
            quickJoinButton = GameObject.Find("QuickJoinButton").GetComponent<Button>();
            joinButton = GameObject.Find("JoinButton").GetComponent<Button>();

            quickJoinButton.onClick.RemoveAllListeners();
            joinButton.onClick.RemoveAllListeners();
            quickJoinButton.onClick.AddListener(QuickJoinMethod);
            joinButton.onClick.AddListener(JoinLobbyWithLobbyCode);
        }
    }
    private void Start()
    {
        _currentLobby = CurrentLobby.Instance;
        playerName = AuthenticationService.Instance.PlayerName;
    }
>>>>>>> parent of 548f87e (Kullanıcı Adı Arayüzü tasarımı gerçekleştirildi)
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
