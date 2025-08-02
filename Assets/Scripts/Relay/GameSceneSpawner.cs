using Unity.Netcode;
using UnityEngine;

public class GameSceneSpawner : MonoBehaviour
{
    public static GameSceneSpawner Instance { get; private set; }
    public GameObject playerPrefab;

    private void Awake()
    {
        Instance = this;
    }
    public void StartPlayerPrefabSpawnControllerEvent()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneName == "Mekan1" && sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            ulong clientId = sceneEvent.ClientId;

            if (NetworkManager.Singleton.IsServer)
            {
                // Her client için özel konumda spawn et
                Vector3 spawnPosition = GetSpawnPositionForClient(clientId);
                Quaternion spawnRotation = Quaternion.identity;

                GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }
        }
    }

    private Vector3 GetSpawnPositionForClient(ulong clientId)
    {
        // Dilersen clientId'ye göre farklı konumlar atayabilirsin
        return new Vector3(0, 0, 0);
    }
}
