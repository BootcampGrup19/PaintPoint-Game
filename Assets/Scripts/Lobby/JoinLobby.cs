using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class JoinLobby : MonoBehaviour
{
    public TMP_InputField lobbyCode;
    public TMP_InputField password;

    public static bool CheckPassword(Lobby lobby, string password)
    {
        var enteredPassword = password;
        if (lobby.Data.TryGetValue("password", out var passwordData))
        {
            string actualPassword = passwordData.Value;

            if (!string.IsNullOrEmpty(actualPassword))
            {
                return enteredPassword == actualPassword;
            }
        }

        // Şifre yoksa herkes girebilir
        return true;
    }

    public async void JoinLobbyWithLobbyCode(string lobbycode)
    {
        var code = lobbyCode.text;
        try
        {
            if (CreateALobby.checkPassword)
            {
                await LobbyService.Instance.JoinLobbyByCodeAsync(code);
                Debug.Log("Joined lobby with code: " + code);
            }
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
            var lobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
            if (lobby.Data.TryGetValue("password", out var passwordData))
            {
                string realPassword = passwordData.Value;

                if (enteredPassword != realPassword)
                {
                    Debug.LogWarning("Wrong Password!");
                    return;
                }
            }

            await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log("Joined lobby with id: " + lobbyId);
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
            Debug.Log("Joined lobby with Quick Join: " + lobby.Id);
            Debug.Log("lobby code: " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
}
