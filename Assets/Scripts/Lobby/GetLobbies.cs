using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GetLobbies : MonoBehaviour
{
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
            Debug.LogWarning("Get Lobbies Done! Count::"+ lobbies.Results.Count);
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
}
