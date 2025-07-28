using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CurrentLobby : MonoBehaviour
{
    public static CurrentLobby Instance { get; private set; }
    public Lobby currentLobby { get; set; }

    public string playerName;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);               // İkinci kopyaya izin verme
            return;
        }
        Instance = this;
        playerName = AuthenticationService.Instance.PlayerName;
        DontDestroyOnLoad(gameObject);         // Sahne atlayınca kalıcı
    }
}
