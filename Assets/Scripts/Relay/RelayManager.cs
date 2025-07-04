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

public class RelayManager : MonoBehaviour
{
    private string playerID;
    private RelayHostData _hostData;
    private RelayJoinData _joinData;
    public TextMeshProUGUI idText;
    public TextMeshProUGUI joinCodeText;
    public TextMeshProUGUI text;
    public TMP_InputField inputField;
    public TMP_Dropdown playerCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("Unity Services Init");
        SignIn();
    }

    async void SignIn()
    {
        Debug.Log("Signing In");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerID = AuthenticationService.Instance.PlayerId;
        Debug.Log("Signed In");
        idText.text = "Player Id: " + playerID;
    }
    public async void OnHostClick()
    {
        int maxPlayerCount = Convert.ToInt32(playerCount.options[playerCount.value].text);

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayerCount);
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
        joinCodeText.text = _hostData.joinCode;

        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_hostData.IPv4Address, _hostData.port, _hostData.AllocationIDBytes, _hostData.key, _hostData.ConnectionData);
        NetworkManager.Singleton.StartHost();
        InitMovementText();
    }
    public async void OnJoinClick()
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(inputField.text);

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
        joinCodeText.text = _hostData.joinCode;

        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(_joinData.IPv4Address, _joinData.port, _joinData.AllocationIDBytes, _joinData.key, _joinData.ConnectionData, _joinData.HostConnectionData);
        NetworkManager.Singleton.StartClient();
        InitMovementText();
    }
    private void InitMovementText()
    {
        if(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            text.text = "Move";
        }
        else if(NetworkManager.Singleton.IsClient)
        {
            text.text = "Request Move";
        }
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
