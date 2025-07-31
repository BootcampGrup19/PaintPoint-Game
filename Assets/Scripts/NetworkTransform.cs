using UnityEngine;
using Unity.Netcode;
namespace Unity.BizimKodlar
{
    public class NetworkTransform : NetworkBehaviour
    {

        // Update is called once per frame
        void Update()
        {
            if (IsOwner && IsServer)
            {
                transform.RotateAround(GetComponentInParent<Transform>().position, Vector3.up, 100f * Time.deltaTime);
            }
        }
    }
}
