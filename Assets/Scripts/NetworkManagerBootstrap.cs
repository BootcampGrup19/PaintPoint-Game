using UnityEngine;
using Unity.Netcode;


namespace Unity.BizimKodlar
{
    [RequireComponent(typeof(NetworkManager))]

    public class NetworkManagerBootstrap : MonoBehaviour
    {
        void Awake()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton != GetComponent<NetworkManager>())
            {                   // Aynı objeden ikinci kez oluşmasın
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }
    }
}
