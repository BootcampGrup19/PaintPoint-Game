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
    public GameObject buttonPrefab;
    public GameObject buttonContainers;
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
                CreateLobbyButton(bulunanLobby);
            }
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    private void CreateLobbyButton(Lobby lobby)
    {
        var button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);

        button.name = lobby.Name;
        button.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Name;

        var recttransform = button.GetComponent<RectTransform>();
        recttransform.SetParent(buttonContainers.transform);
        
        button.GetComponent<Button>().onClick.AddListener(delegate () { Lobby_OnClick(lobby); });
    }
    public void Lobby_OnClick(Lobby lobby)
    {
        Debug.Log("Clicked Lobby " + lobby.Name);
        GetComponent<JoinLobby>().JoinLobbyWithLobbyId(lobby.Id);
    }
    private void ClearContainer()
    {
        if (buttonContainers is not null && buttonContainers.transform.childCount > 0)
        {
            foreach (Transform variable in buttonContainers.transform)
            {
                Destroy(variable.gameObject);
            }
        }
    }
}
