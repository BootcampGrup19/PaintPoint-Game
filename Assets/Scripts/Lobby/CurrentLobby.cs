using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CurrentLobby : MonoBehaviour
{
    public static CurrentLobby Instance;
    public Lobby currentLobby { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(this.gameObject); 
    }
}
