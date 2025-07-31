using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        lobbyId = _currentLobby.currentLobby.Id;
        InvokeRepeating(nameof(PollForLobbyUpdate), 1.1f, 2f);
        PopulateUIElements();
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
                text.GetComponent<TextMeshProUGUI>().text = player.Id;
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
                text.GetComponent<TextMeshProUGUI>().text = player.Id;
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
                text.GetComponent<TextMeshProUGUI>().text = player.Id;
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
        text.GetComponent<TextMeshProUGUI>().text = player.Id;
        
        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.SetParent(container.transform);
    }
}
