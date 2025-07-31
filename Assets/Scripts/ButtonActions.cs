using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace Unity.BizimKodlar
{
    public class ButtonActions : MonoBehaviour
    {

        private NetworkManager networkManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        }

        public void SubmitNewPosition()
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

            if (playerObject == null)
            {
                Debug.LogWarning("Local player object not found.");
                return;
            }

            var player = playerObject.GetComponent<PlayerMovement>();

            if (player == null)
            {
                Debug.LogWarning("PlayerMovement component not found.");
                return;
            }

            player.Move();
        }
    }
}
