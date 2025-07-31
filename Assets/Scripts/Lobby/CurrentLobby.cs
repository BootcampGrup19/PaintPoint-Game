using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Unity.BizimKodlar
{
    public class CurrentLobby : MonoBehaviour
    {
        public static CurrentLobby Instance { get; private set; }
        public Lobby currentLobby { get; set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);               // İkinci kopyaya izin verme
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);         // Sahne atlayınca kalıcı
        }
    }
}
