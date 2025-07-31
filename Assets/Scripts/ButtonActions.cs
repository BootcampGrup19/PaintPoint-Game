using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ButtonActions : MonoBehaviour
{

    private NetworkManager networkManager;
    public TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager = GetComponentInParent<NetworkManager>();
    }

    public void StartHost()
    {
        networkManager.StartHost();
        InitMovementText();
    }

    public void StartClient()
    {
        networkManager.StartClient();
        InitMovementText();
    }
    public void SubmitNewPosition()
    {
        var playerObject =NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var player = playerObject.GetComponent<PlayerMovement>();
        player.Move();
    }

    private void InitMovementText()
    {
        if(networkManager.IsServer || networkManager.IsHost)
        {
            text.text = "Move";
        }
        else if(networkManager.IsClient)
        {
            text.text = "Request Move";
        }
    }


}
