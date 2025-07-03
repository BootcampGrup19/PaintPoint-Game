using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class PlayerMovement : NetworkBehaviour
{
    public NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            Move();
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = position.Value;
    }
    public void Move()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            position.Value = randomPosition;
        }
        else 
        {
            SubmitPositionRequestServerRpc();
        }
    }
    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        position.Value = GetRandomPositionOnPlane();
    }
    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }
}
