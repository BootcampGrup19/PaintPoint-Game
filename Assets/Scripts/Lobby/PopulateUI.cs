using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using System;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class PopulateUI : MonoBehaviour
{
    public TextMeshProUGUI lobbyName;
    public TextMeshProUGUI lobbyCode;
    private CurrentLobby _currentLobby;
    private CreateALobby createALobby;
    private string lobbyId;
    private string playerTeam;
    public GameObject playerInfoContainer;
    public GameObject redTeamContainer;
    public GameObject blueTeamContainer;
    public GameObject playerInfoPrefab;
    public Button startReadyButton;
    public TextMeshProUGUI startReadyButtonText;
    private bool isReady = false;
    bool _relayJoined = false;
    string _lastHostId;
    bool   _iAmMigratedHost = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (CurrentLobby.Instance.currentLobby == null)
        {
            Debug.LogError("PopulateUI → CurrentLobby boş! LobbyBrowser sahnesine dönüyorum.");
            SceneManager.LoadScene("LobbyBrowserScene");
            return;
        }
        _currentLobby = CurrentLobby.Instance;
        createALobby = GameObject.Find("LobbyManager").GetComponent<CreateALobby>();

        lobbyId = _currentLobby.currentLobby.Id;

        InvokeRepeating(nameof(PollForLobbyUpdate), 1.1f, 3f);
        UpdateStartReadyButtonUI();
        PopulateUIElements();
    }
    void UpdateStartReadyButtonUI()
    {
        if (IsHost())
        {
            startReadyButtonText.text = "Start";
            startReadyButton.interactable = false;
            startReadyButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            startReadyButtonText.text = "Ready";
            startReadyButton.onClick.AddListener(OnReadyButtonClicked);
        }
    }
    void PopulateUIElements()
    {
        ClearAllTeamContainers();
        lobbyName.text = _currentLobby.currentLobby.Name;
        lobbyCode.text = "Lobby Code: " + _currentLobby.currentLobby.LobbyCode;
        foreach (Player player in _currentLobby.currentLobby.Players)
        {
            if (player.Data != null && player.Data.TryGetValue("team", out var teamData))
            {
                if (teamData.Value == "red")
                    AddPlayerToContainer(player, redTeamContainer);
                else if (teamData.Value == "blue")
                    AddPlayerToContainer(player, blueTeamContainer);
                else if (teamData.Value == "none")
                    AddPlayerToContainer(player, playerInfoContainer);
            }
            else
            {
                // Takımsız oyuncu
                AddPlayerToContainer(player, playerInfoContainer);
            }
            CheckIfAllReady();
        }
    }
    async void PollForLobbyUpdate()
    {
        if (string.IsNullOrEmpty(lobbyId) || _currentLobby.currentLobby == null || !gameObject.activeInHierarchy) return;

         try
        {
            _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Lobby artik yok → PollForLobbyUpdate durduruldu. " + e);
            CancelInvoke(nameof(PollForLobbyUpdate)); // Kendini durdur
            return;
        }

        RelayManager.Instance.LobbyId = lobbyId;
        PopulateUIElements();

            /* ---------- ❶ host‑migration detection ---------- */
        if (_lastHostId != null && _currentLobby.currentLobby.HostId != _lastHostId)
        {
            Debug.Log($"Host changed → new={_currentLobby.currentLobby.HostId}");
            if (_currentLobby.currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                _iAmMigratedHost = true;                        // I’m the new host
        }
        _lastHostId = _currentLobby.currentLobby.HostId;

        /* ---------- ❷ if I became host, start new Relay ---------- */
        if (_iAmMigratedHost)
        {
            _iAmMigratedHost = false;                          // run once
            int maxPlayers = int.Parse(_currentLobby.currentLobby.Data["maxplayers"].Value);
            RelayManager.Instance.LobbyId = lobbyId;
            await RelayManager.Instance.BecomeNewHostAsync(maxPlayers, lobbyId);
        }

        /* ---------- ❸ clients join relay when code appears ---------- */
        if (!_relayJoined && !IsHost() &&
            _currentLobby.currentLobby.Data.TryGetValue("relayJoinCode", out var codeObj))
        {
            _relayJoined = true;
            await RelayManager.Instance.JoinRelayAsync(codeObj.Value);
        }
    }
    private void ClearAllTeamContainers()
    {
        foreach (Transform child in redTeamContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in blueTeamContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in playerInfoContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void JoinRedTeam()
    {
        ClearAllTeamContainers();
        playerTeam = "red";
        UpdatePlayerTeam();
        string currentPlayerId = AuthenticationService.Instance.PlayerId;
        foreach (Player player in _currentLobby.currentLobby.Players)
        {
            if (player.Id == currentPlayerId)
            {
                var text = Instantiate(playerInfoPrefab, Vector3.zero, Quaternion.identity);
                text.name = player.Joined.ToShortTimeString();
                text.GetComponentInChildren<TextMeshProUGUI>().text = player.Id;
                var rectTransform = text.GetComponent<RectTransform>();
                rectTransform.SetParent(redTeamContainer.transform);
            }
        }
    }
    public void JoinBlueTeam()
    {
        ClearAllTeamContainers();
        playerTeam = "blue";
        UpdatePlayerTeam();
        string currentPlayerId = AuthenticationService.Instance.PlayerId;
        foreach (Player player in _currentLobby.currentLobby.Players)
        {
            if (player.Id == currentPlayerId)
            {
                var text = Instantiate(playerInfoPrefab, Vector3.zero, Quaternion.identity);
                text.name = player.Joined.ToShortTimeString();
                text.GetComponentInChildren<TextMeshProUGUI>().text = player.Id;
                var rectTransform = text.GetComponent<RectTransform>();
                rectTransform.SetParent(blueTeamContainer.transform);
            }
        }
    }
    public void JoinPlayerInfoArea()
    {
        ClearAllTeamContainers();
        playerTeam = "none";
        UpdatePlayerTeam();
        string currentPlayerId = AuthenticationService.Instance.PlayerId;
        foreach (Player player in _currentLobby.currentLobby.Players)
        {
            if (player.Id == currentPlayerId)
            {
                var text = Instantiate(playerInfoPrefab, Vector3.zero, Quaternion.identity);
                text.name = player.Joined.ToShortTimeString();
                text.GetComponentInChildren<TextMeshProUGUI>().text = player.Id;
                var rectTransform = text.GetComponent<RectTransform>();
                rectTransform.SetParent(playerInfoContainer.transform);
            }
        }
    }
    async void UpdatePlayerTeam()
    {
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            options.Data = new Dictionary<string, PlayerDataObject>()
            {
                {"team", new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member, value: playerTeam)}
            };
            await LobbyService.Instance.UpdatePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId, options);
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    void AddPlayerToContainer(Player player, GameObject container)
    {
        var text = Instantiate(playerInfoPrefab, Vector3.zero, Quaternion.identity);

        text.name = player.Joined.ToShortTimeString();
        text.GetComponentInChildren<TextMeshProUGUI>().text = player.Id;

        var readyTextgo = text.transform.Find("PlayerReadyText").gameObject;

        if (player.Id != _currentLobby.currentLobby.HostId)
        {
            readyTextgo.SetActive(true);
            var readyText = text.transform.Find("PlayerReadyText").GetComponent<TextMeshProUGUI>();
            if (readyText != null && player.Data != null && player.Data.ContainsKey("ready"))
            {
                readyText.text = player.Data["ready"].Value == "true" ? "Ready" : "UnReady";
            }
        }

        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.SetParent(container.transform);
    }
    bool IsHost()
    {
        return _currentLobby.currentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    void CheckIfAllReady()
    {
        if (!IsHost()) return;

        bool allReady = _currentLobby.currentLobby.Players
            .Where(p => p.Id != AuthenticationService.Instance.PlayerId)
            .All(p => p.Data != null && p.Data.ContainsKey("ready") && p.Data["ready"].Value == "true");

        startReadyButton.interactable = allReady;
    }
    public void OnReadyButtonClicked()
    {
        isReady = !isReady;
        SetReadyStatus(isReady);
        startReadyButtonText.text = isReady ? "UnReady" : "Ready";
    }
    async void SetReadyStatus(bool isReady)
    {
        var options = new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady ? "true" : "false") }
            }
        };

        await LobbyService.Instance.UpdatePlayerAsync(
            _currentLobby.currentLobby.Id,
            AuthenticationService.Instance.PlayerId,
            options
        );
    }
    public async void OnStartButtonClicked()
    {
        if (!IsHost())
        {
            Debug.Log(IsHost());
            return;
        }
        startReadyButton.interactable = false;

        int maxConnections = Convert.ToInt32(_currentLobby.currentLobby.Data["maxplayers"].Value);
        RelayManager.Instance.LobbyId = lobbyId;
        string joinCode = await RelayManager.Instance.StartRelayHostAsync(maxConnections);

        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                ["relayJoinCode"] = new DataObject(DataObject.VisibilityOptions.Public, joinCode),
                ["startTime"] = new DataObject(DataObject.VisibilityOptions.Public,
                                                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
            }
        });

        Debug.Log("Start Button Clicked");

        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        Debug.Log("Countdown started.");
        int countdown = 3;
        while (countdown > 0)
        {
            startReadyButtonText.text = countdown.ToString();
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        Debug.Log("Countdown finished. Loading scene...");

        startReadyButtonText.text = "Starting...";
        yield return new WaitForSeconds(0.5f); // opsiyonel

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(
        "MultiplayerScene",
        UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    public async void QuitLobby()
    {
        createALobby.StopHeartbeatCoroutine();
        CancelInvoke(nameof(PollForLobbyUpdate));

        string pid = AuthenticationService.Instance.PlayerId;
        bool amHost = IsHost();

        if (amHost && _currentLobby.currentLobby.Players.Count == 1)
        {
            Debug.Log("Players :" + _currentLobby.currentLobby.Players.Count);
            // await LobbyService.Instance.UpdateLobbyAsync(
            //     lobbyId,
            //     new UpdateLobbyOptions
            //     {
            //         Data = new Dictionary<string, DataObject>
            //         {
            //             ["status"] = new DataObject(DataObject.VisibilityOptions.Public,
            //             "closed",
            //             DataObject.IndexOptions.S1)
            //         }
            //     });

            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);       // last player → delete
        }
        else
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, pid); // leave but keep lobby
        }

        NetworkManager.Singleton.Shutdown();                             // stop NGO / Relay

        _currentLobby.currentLobby = null;
        RelayManager.Instance.LobbyId = null;

        SceneManager.LoadScene("LobbyBrowserScene");
    }
}