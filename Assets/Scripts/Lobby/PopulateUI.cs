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
    private string lobbyId;
    private string playerTeam;
    public GameObject playerInfoContainer;
    public GameObject redTeamContainer;
    public GameObject blueTeamContainer;
    public GameObject playerInfoPrefab;
    public Button startReadyButton;
    public TextMeshProUGUI startReadyButtonText;
    private bool isReady = false;
    private RelayHostData _hostData;
    bool _relayJoined = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        lobbyId = _currentLobby.currentLobby.Id;
        InvokeRepeating(nameof(PollForLobbyUpdate), 1.1f, 2f);
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
    // void CreatePlayerInfoCard(Player player)
    // {
    //     if (player.Data != null && player.Data.ContainsKey("team"))
    //     {
    //         return; // Takımı zaten belli olan oyuncuyu atla
    //     }

    //     var text = Instantiate(playerInfoPrefab, Vector3.zero, Quaternion.identity);

    //     text.name = player.Joined.ToShortTimeString();
    //     text.GetComponent<TextMeshProUGUI>().text = player.Id;

    //     var rectTransform = text.GetComponent<RectTransform>();
    //     rectTransform.SetParent(playerInfoContainer.transform);
    // }
    async void PollForLobbyUpdate()
    {
        _currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
        PopulateUIElements();

        if (!_relayJoined && _currentLobby.currentLobby.Data.TryGetValue("relayJoinCode", out var codeObj))
        {
            string joinCode = codeObj.Value;
            await JoinRelayAndStartClientAsync(joinCode);
            _relayJoined = true; // Tek seferlik
        }
    }
    async Task JoinRelayAndStartClientAsync(string joinCode)
    {
        var alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetRelayServerData(alloc.RelayServer.IpV4, (ushort)alloc.RelayServer.Port,
                               alloc.AllocationIdBytes, alloc.Key,
                               alloc.ConnectionData, alloc.HostConnectionData);

        NetworkManager.Singleton.StartClient();      // Host sahneyi çoktan yüklemiş olsa bile,
        // Netcode SceneManager client'ı o sahneye otomatik senkronize eder.
    }
    // private void ClearContainer()
    // {
    //     if (playerInfoContainer is not null && playerInfoContainer.transform.childCount > 0)
    //     {
    //         foreach (Transform variable in playerInfoContainer.transform)
    //         {
    //             Destroy(variable.gameObject);
    //         }
    //     }
    //     if (redTeamContainer is not null && redTeamContainer.transform.childCount > 0)
    //     {
    //         foreach (Transform variable in redTeamContainer.transform)
    //         {
    //             Destroy(variable.gameObject);
    //         }
    //     }
    //     if (blueTeamContainer is not null && blueTeamContainer.transform.childCount > 0)
    //     {
    //         foreach (Transform variable in blueTeamContainer.transform)
    //         {
    //             Destroy(variable.gameObject);
    //         }
    //     }
    // }
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
        await CreateRelayAndStartHostAsync(maxConnections);

        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                ["relayJoinCode"] = new DataObject(DataObject.VisibilityOptions.Public, _hostData.joinCode),
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

        NetworkManager.Singleton.SceneManager.LoadScene(
        "MultiplayerScene",
        UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    async Task CreateRelayAndStartHostAsync(int maxConnections)
    {
        Debug.Log("Creating Relay Allocation...");
        Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        Debug.Log("Relay Allocation Created.");

        _hostData = new RelayHostData()
        {
            IPv4Address = alloc.RelayServer.IpV4,
            port = (ushort)alloc.RelayServer.Port,
            AllocationID = alloc.AllocationId,
            AllocationIDBytes = alloc.AllocationIdBytes,
            ConnectionData = alloc.ConnectionData,
            key = alloc.Key,
            joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId)
        };

        Debug.Log($"Join code: {_hostData.joinCode}");

        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetRelayServerData(
            _hostData.IPv4Address,
            _hostData.port,
            _hostData.AllocationIDBytes,
            _hostData.key,
            _hostData.ConnectionData);

        Debug.Log("Starting Host...");
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started.");
    }
}