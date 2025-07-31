using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


public class GetLobbies : MonoBehaviour
{
    public GameObject lobbyRowPrefab;
    public GameObject rowContainer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void GetLobbiesTest()
    {
        ClearContainer();
        try
        {
            QueryLobbiesOptions options = new();
            Debug.LogWarning("QueryLobbiesTest");
            options.Count = 25;

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
            Debug.LogWarning("Get Lobbies Done! Count::" + lobbies.Results.Count);

            foreach (Lobby bulunanLobby in lobbies.Results)
            {
                Debug.Log("Lobby Name: " + bulunanLobby.Name + "\n" +
                "Time for Created Lobby: " + bulunanLobby.Created + "\n" +
                "Lobby Code: " + bulunanLobby.LobbyCode);
                CreateLobbyRow(bulunanLobby);
            }
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    private void CreateLobbyRow(Lobby lobby)
    {
        GameObject row = Instantiate(lobbyRowPrefab, rowContainer.transform);
        row.name = lobby.Name;

        row.transform.Find("LobbyNameText").GetComponent<TextMeshProUGUI>().text = lobby.Name;
        row.transform.Find("OwnerText").GetComponent<TextMeshProUGUI>().text = lobby.HostId;
        row.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        var rectTransform = row.GetComponent<RectTransform>();
        rectTransform.SetParent(rowContainer.transform);
        
        Button joinButton = row.transform.Find("JoinButton").GetComponent<Button>();
        joinButton.onClick.AddListener(() => Lobby_OnClick(lobby));
    }
    public void Lobby_OnClick(Lobby lobby)
    {
        Debug.Log("Clicked Lobby " + lobby.Name);
        GetComponent<JoinLobby>().JoinLobbyWithLobbyId(lobby.Id);
    }
    private void ClearContainer()
    {
        if (rowContainer is not null && rowContainer.transform.childCount > 0)
        {
            foreach (Transform variable in rowContainer.transform)
            {
                Destroy(variable.gameObject);
            }
        }
    }
}
