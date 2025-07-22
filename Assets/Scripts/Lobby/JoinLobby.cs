using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class JoinLobby : MonoBehaviour
{
    public TMP_InputField lobbyCode;
    public TMP_InputField password;
    public Button quickJoinButton;
    public Button joinButton;
    private CurrentLobby _currentLobby;

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
    }
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
            _currentLobby.currentLobby = lobby;

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
            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogError("Lobby Id bilgisi eksik");
                return;
            }

            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();

            if (!string.IsNullOrEmpty(enteredPassword))
            {
                options.Password = enteredPassword;
            }

            Lobby lobby1 = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

            if (lobby1 == null)
            {
                Debug.LogError("Join işlemi başarisiz oldu: Lobby1 null döndü.");
                return;
            }

            _currentLobby.currentLobby = lobby1;
            RelayManager.Instance.LobbyId = lobby1.Id;

            DontDestroyOnLoad(this);

            Debug.Log("Join işlemi başarili. Lobby ID: " + lobby1.Id);

            Debug.Log("Joined lobby with id: " + lobbyId);

            SceneManager.sceneLoaded += OnLobbyRoomLoaded;
            SceneManager.LoadScene("LobbyRoom");
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    private void OnLobbyRoomLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnLobbyRoomLoaded; // Event'i sil

        if (scene.name != "LobbyRoom") return;

        StartCoroutine(DelayedPopulateUI());
    }
    private IEnumerator DelayedPopulateUI()
    {
        yield return null;
        yield return null;

        var ui = FindAnyObjectByType<PopulateUI>();
        if (ui != null)
        {
            ui.PopulateUIElements();
        }
        else
        {
            Debug.LogWarning("PopulateUI component not found.");
        }

        // ✅ relayJoinCode'u çek
        //var player = _currentLobby.currentLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);
        if (_currentLobby.currentLobby.Data.TryGetValue("relayJoinCode", out var relayCodeObj))
        {
            string relayJoinCode = relayCodeObj.Value;
            Debug.Log("RelayJoinCode lobby'den bulundu: " + relayCodeObj);
            var task = RelayManager.Instance.JoinRelayAsync(relayJoinCode);

            while (!task.IsCompleted) yield return null;

            if (task.Exception != null)
                Debug.LogError("Relay'e bağlanirken hata: " + task.Exception.Message);
        }
        else
        {
            Debug.Log("relayJoinCode Lobby.Data'da bulunamadi. Host henüz başlatmamiş olabilir.");
        }

        /*if (PopulateUI.Instance != null)
        {
            PopulateUI.Instance.SubscribeToLobbyChanges(); // 🔄 UI güncellemelerini başlat
        }
        else
        {
            Debug.LogWarning("PopulateUI.Instance null → SubscribeToLobbyChanges yapilamadi.");
        }*/
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
