using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Services.Relay;
using NUnit.Framework.Constraints;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    public string LobbyId { get; set; }
    private RelayHostData _hostData;
    private RelayJoinData _joinData;
    [SerializeField] public TextMeshProUGUI joinCodeText;
    [SerializeField] public TextMeshProUGUI text;
    [SerializeField] public TextMeshProUGUI playerIDText;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // MultiplayerScene yüklendiğinde UI referanslarını güncelle
        if (scene.name == "MultiplayerScene")
        {
            playerIDText = GameObject.Find("PlayerID").GetComponent<TextMeshProUGUI>();
            joinCodeText = GameObject.Find("JoinCode").GetComponent<TextMeshProUGUI>();
            text = GameObject.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("Unity Services Init");
        // SignIn();
        SafeSetText(playerIDText, "PlayerID: " + AuthenticationService.Instance.PlayerId);
    }

    // async void SignIn() 
    // {
    //     Debug.Log("Signing In");
    //     await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //     playerID = AuthenticationService.Instance.PlayerId;
    //     Debug.Log("Signed In");
    //     idText.text = "Player Id: " + playerID;
    // }
    public async Task<string> StartRelayHostAsync(int maxPlayers)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        _hostData = new RelayHostData()
        {
            IPv4Address = allocation.RelayServer.IpV4,
            port = (ushort)allocation.RelayServer.Port,

            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            key = allocation.Key
        };
        _hostData.joinCode = await RelayService.Instance.GetJoinCodeAsync(_hostData.AllocationID);
        Debug.Log("Allocation Complete: " + _hostData.AllocationID);
        Debug.LogWarning("Join Code: " + _hostData.joinCode);

        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_hostData.IPv4Address, _hostData.port, _hostData.AllocationIDBytes, _hostData.key, _hostData.ConnectionData);
        
        InitMovementText();

        NetworkManager.Singleton.StartHost();

        Debug.Log("Relay host başlatildi: " + AuthenticationService.Instance.PlayerId);

        await LobbyService.Instance.UpdatePlayerAsync(
        LobbyId,
        AuthenticationService.Instance.PlayerId,
        new UpdatePlayerOptions { AllocationId = allocation.AllocationId.ToString() });

        SafeSetText(joinCodeText, "Join Code: " + _hostData.joinCode);

        await LobbyService.Instance.UpdateLobbyAsync(
        LobbyId,
        new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, _hostData.joinCode) },
                { "startTime", new DataObject(DataObject.VisibilityOptions.Public, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()) }
            }
        });

        return _hostData.joinCode;
    }
    public async Task JoinRelayAsync(string joinCode)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        _joinData = new RelayJoinData()
        {
            IPv4Address = allocation.RelayServer.IpV4,
            port = (ushort)allocation.RelayServer.Port,

            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            key = allocation.Key
        };
        Debug.Log("Join Succes :" + _joinData.AllocationID);
        SafeSetText(joinCodeText, "Join Code: " + _hostData.joinCode);

        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_joinData.IPv4Address, _joinData.port, _joinData.AllocationIDBytes, _joinData.key, _joinData.ConnectionData, _joinData.HostConnectionData);
        NetworkManager.Singleton.StartClient();

        Debug.Log("Relay host başlatildi: " + AuthenticationService.Instance.PlayerId);

        await LobbyService.Instance.UpdatePlayerAsync(
        LobbyId,
        AuthenticationService.Instance.PlayerId,
        new UpdatePlayerOptions { AllocationId = _joinData.AllocationID.ToString() });

        InitMovementText();
    }
    private void InitMovementText()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            SafeSetText(text, "Move");
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            SafeSetText(text, "Request Move");
        }
    }
    void SafeSetText(TextMeshProUGUI t, string v)
    {
        if (t != null) t.text = v;
    }
    public void StopRelay()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    public async Task<string> RestartRelayHostAsync(int maxPlayers)
    {
        StopRelay(); // Önce varsa bağlantıyı kapat (NetworkManager shutdown vs.)
        string joinCode = await StartRelayHostAsync(maxPlayers);
        return joinCode;
    }
    public async Task BecomeNewHostAsync(int maxPlayers, string lobbyId)
    {
        // 1. start a fresh Relay allocation
        string newJoinCode = await RestartRelayHostAsync(maxPlayers);

        // 2. write Allocation‑ID & joinCode into Lobby so clients can reconnect
        await LobbyService.Instance.UpdatePlayerAsync(
            lobbyId,
            AuthenticationService.Instance.PlayerId,
            new UpdatePlayerOptions
            {
                AllocationId = _hostData.AllocationID.ToString(),          // Relay integration:contentReference[oaicite:0]{index=0}
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "relayJoinCode",
                      new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newJoinCode) }
                }
            });

        // 3. promote myself to lobby host
        await LobbyService.Instance.UpdateLobbyAsync(
        lobbyId,
        new UpdateLobbyOptions
        {
            HostId = AuthenticationService.Instance.PlayerId,
            Data = new Dictionary<string, DataObject>
            {
                { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, newJoinCode) },
                { "startTime", new DataObject(DataObject.VisibilityOptions.Public, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()) }
            }
        });
    }
}
public struct RelayHostData
{
    public string joinCode;
    public string IPv4Address;
    public ushort port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] key;
}
public struct RelayJoinData
{
    public string IPv4Address;
    public ushort port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public byte[] key;
}
